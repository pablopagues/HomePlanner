using System.Text;
using Application.HomePlanner.Common;
using Application.HomePlanner.Middleware;
using Application.HomePlanner.Services.Auth;
using Application.HomePlanner.Services.Email;
using Domain.HomePlanner.Models.SaaS.Identity;
using Infrastructure.HomePlanner.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Infrastructure.HomePlanner.Services.Auth;

/// <inheritdoc cref="ISenhaService"/>
public class SenhaService : ISenhaService
{
    /// <summary>Pedidos de reset aceitos por e-mail dentro da janela, antes de passar a ignorar.</summary>
    private const int MaxPedidosPorJanela = 3;
    private static readonly TimeSpan JanelaThrottle = TimeSpan.FromMinutes(15);

    private readonly UserManager<Usuario> _userManager;
    private readonly IEmailService _emailService;
    private readonly TenantContext _tenantContext;
    private readonly HomePlannerDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SenhaService> _logger;

    public SenhaService(
        UserManager<Usuario> userManager,
        IEmailService emailService,
        TenantContext tenantContext,
        HomePlannerDbContext db,
        IMemoryCache cache,
        ILogger<SenhaService> logger)
    {
        _userManager = userManager;
        _emailService = emailService;
        _tenantContext = tenantContext;
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<ResultadoOperacao> SolicitarResetAsync(
        string email, string baseUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return ResultadoOperacao.Falha(ErrosApp.InformeEmail);

        var emailNormalizado = email.Trim().ToUpperInvariant();

        // Throttle por e-mail: evita usar o produto como máquina de spam contra terceiros.
        // Silencioso de propósito — avisar "muitas tentativas" já revelaria que a conta existe.
        var chave = $"reset_senha_{emailNormalizado}";
        var tentativas = _cache.Get<int?>(chave) ?? 0;
        if (tentativas >= MaxPedidosPorJanela)
        {
            _logger.LogWarning("Reset de senha ignorado por throttle.");
            return ResultadoOperacao.Ok();
        }
        _cache.Set(chave, tentativas + 1, JanelaThrottle);

        var usuario = await _db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.NormalizedEmail == emailNormalizado, ct);

        // Só manda para conta existente, ativa e com e-mail já confirmado — quem nunca
        // confirmou usa o fluxo de confirmação, não o de reset.
        if (usuario is not null && usuario.Ativo && !usuario.IsDeleted && usuario.EmailConfirmed)
        {
            _tenantContext.Definir(usuario.TenantId, usuario.Id, usuario.NomeCompleto);

            var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
            var tokenCodificado = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var link = $"{baseUrl.TrimEnd('/')}/Identity/Account/RedefinirSenha" +
                       $"?userId={Uri.EscapeDataString(usuario.Id)}&token={tokenCodificado}";

            await _emailService.EnviarResetSenhaAsync(usuario.Email!, usuario.NomeCompleto, link, ct);
            _logger.LogInformation("E-mail de reset de senha enviado para o usuário {UsuarioId}.", usuario.Id);
        }

        // Resposta idêntica nos dois casos.
        return ResultadoOperacao.Ok();
    }

    public async Task<ResultadoOperacao> RedefinirAsync(
        string usuarioId, string token, string novaSenha, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(usuarioId) || string.IsNullOrWhiteSpace(token))
            return ResultadoOperacao.Falha(ErrosApp.LinkExpirado);

        var usuario = await _db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == usuarioId, ct);

        if (usuario is null || !usuario.Ativo || usuario.IsDeleted)
            return ResultadoOperacao.Falha(ErrosApp.LinkExpirado);

        _tenantContext.Definir(usuario.TenantId, usuario.Id, usuario.NomeCompleto);

        string tokenDecodificado;
        try
        {
            tokenDecodificado = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        }
        catch (FormatException)
        {
            return ResultadoOperacao.Falha(ErrosApp.LinkExpirado);
        }

        var resultado = await _userManager.ResetPasswordAsync(usuario, tokenDecodificado, novaSenha);
        if (!resultado.Succeeded)
            return ResultadoOperacao.Falha(resultado.Errors.Select(e => e.Description).ToArray());

        _logger.LogInformation("Senha redefinida para o usuário {UsuarioId}.", usuario.Id);
        return ResultadoOperacao.Ok();
    }

    public async Task<ResultadoOperacao> AlterarAsync(
        string senhaAtual, string novaSenha, CancellationToken ct = default)
    {
        var usuarioId = _tenantContext.UsuarioId;
        if (string.IsNullOrEmpty(usuarioId))
            return ResultadoOperacao.Falha(ErrosApp.SessaoExpirada);

        var usuario = await _db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == usuarioId, ct);

        if (usuario is null || !usuario.Ativo || usuario.IsDeleted)
            return ResultadoOperacao.Falha(ErrosApp.SessaoExpirada);

        var resultado = await _userManager.ChangePasswordAsync(usuario, senhaAtual, novaSenha);
        if (!resultado.Succeeded)
            return ResultadoOperacao.Falha(resultado.Errors.Select(e => e.Description).ToArray());

        _logger.LogInformation("Senha alterada pelo próprio usuário {UsuarioId}.", usuario.Id);
        return ResultadoOperacao.Ok();
    }
}
