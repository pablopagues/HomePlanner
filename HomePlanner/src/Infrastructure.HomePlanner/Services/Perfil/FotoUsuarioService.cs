using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Perfil;
using Application.HomePlanner.Middleware;
using Application.HomePlanner.Services.Perfil;
using Domain.HomePlanner.Models.SaaS.Identity;
using Infrastructure.HomePlanner.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.HomePlanner.Services.Perfil;

public class FotoUsuarioService : IFotoUsuarioService
{
    private readonly UserManager<Usuario> _userManager;
    private readonly HomePlannerDbContext _db;
    private readonly TenantContextAccessor _tenantAccessor;
    private readonly TenantContext _tenantContext;
    private readonly ILogger<FotoUsuarioService> _logger;

    public FotoUsuarioService(
        UserManager<Usuario> userManager,
        HomePlannerDbContext db,
        TenantContextAccessor tenantAccessor,
        TenantContext tenantContext,
        ILogger<FotoUsuarioService> logger)
    {
        _userManager = userManager;
        _db = db;
        _tenantAccessor = tenantAccessor;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public long TamanhoMaximoBytes => 2 * 1024 * 1024; // 2 MB

    public IReadOnlyList<string> TiposPermitidos { get; } =
        ["image/jpeg", "image/png", "image/webp", "image/gif"];

    public async Task<ResultadoOperacao<string>> AtualizarFotoAsync(
        byte[] conteudo, string contentType, CancellationToken ct = default)
    {
        if (conteudo is null || conteudo.Length == 0)
            return ResultadoOperacao<string>.Falha("Arquivo vazio.");

        if (conteudo.Length > TamanhoMaximoBytes)
            return ResultadoOperacao<string>.Falha("A imagem excede o tamanho máximo permitido (2 MB).");

        var tipo = (contentType ?? string.Empty).Trim().ToLowerInvariant();
        if (!TiposPermitidos.Contains(tipo))
            return ResultadoOperacao<string>.Falha("Formato inválido. Use JPG, PNG, WEBP ou GIF.");

        var usuario = await ObterUsuarioAsync();
        if (usuario is null)
            return ResultadoOperacao<string>.Falha("Sessão inválida. Faça login novamente.");

        usuario.Foto = conteudo;
        usuario.FotoContentType = tipo;
        usuario.FotoAtualizadaEm = DateTime.UtcNow;

        var r = await _userManager.UpdateAsync(usuario);
        if (!r.Succeeded)
            return ResultadoOperacao<string>.Falha(r.Errors.Select(e => e.Description).ToArray());

        _logger.LogInformation("Foto de perfil atualizada para usuário {UsuarioId}.", usuario.Id);
        return ResultadoOperacao<string>.Ok(MontarVersao(usuario.FotoAtualizadaEm));
    }

    public async Task<ResultadoOperacao> RemoverFotoAsync(CancellationToken ct = default)
    {
        var usuario = await ObterUsuarioAsync();
        if (usuario is null)
            return ResultadoOperacao.Falha("Sessão inválida. Faça login novamente.");

        usuario.Foto = null;
        usuario.FotoContentType = null;
        usuario.FotoAtualizadaEm = null;

        var r = await _userManager.UpdateAsync(usuario);
        if (!r.Succeeded)
            return ResultadoOperacao.Falha(r.Errors.Select(e => e.Description).ToArray());

        _logger.LogInformation("Foto de perfil removida para usuário {UsuarioId}.", usuario.Id);
        return ResultadoOperacao.Ok();
    }

    public async Task<string?> ObterVersaoFotoAsync(CancellationToken ct = default)
    {
        var userId = await ObterUsuarioIdAsync();
        if (userId is null) return null;

        // Projeta só a coluna de versão — não carrega o blob.
        var info = await _db.Users
            .Where(u => u.Id == userId)
            .Select(u => new { u.FotoAtualizadaEm })
            .FirstOrDefaultAsync(ct);

        return info?.FotoAtualizadaEm is null ? null : MontarVersao(info.FotoAtualizadaEm);
    }

    public async Task<FotoUsuarioDTO?> ObterConteudoFotoAsync(CancellationToken ct = default)
    {
        var userId = await ObterUsuarioIdAsync();
        return userId is null ? null : await CarregarConteudoAsync(userId, ct);
    }

    public async Task<FotoUsuarioDTO?> ObterConteudoFotoAsync(string usuarioId, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(usuarioId)) return null;

        // Garante o tenant hidratado para que o filtro global restrinja a consulta à própria família.
        await _tenantAccessor.GarantirHidratadoAsync();
        return await CarregarConteudoAsync(usuarioId, ct);
    }

    private async Task<FotoUsuarioDTO?> CarregarConteudoAsync(string usuarioId, CancellationToken ct)
    {
        var info = await _db.Users
            .Where(u => u.Id == usuarioId)
            .Select(u => new { u.Foto, u.FotoContentType, u.FotoAtualizadaEm })
            .FirstOrDefaultAsync(ct);

        if (info?.Foto is null || info.Foto.Length == 0)
            return null;

        return new FotoUsuarioDTO
        {
            Conteudo = info.Foto,
            ContentType = string.IsNullOrWhiteSpace(info.FotoContentType)
                ? "application/octet-stream"
                : info.FotoContentType,
            Versao = MontarVersao(info.FotoAtualizadaEm),
        };
    }

    private static string MontarVersao(DateTime? atualizadaEm) =>
        (atualizadaEm ?? DateTime.UnixEpoch).Ticks.ToString();

    private async Task<string?> ObterUsuarioIdAsync()
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        var userId = _tenantContext.UsuarioId;
        return string.IsNullOrEmpty(userId) ? null : userId;
    }

    private async Task<Usuario?> ObterUsuarioAsync()
    {
        var userId = await ObterUsuarioIdAsync();
        return userId is null ? null : await _userManager.FindByIdAsync(userId);
    }
}
