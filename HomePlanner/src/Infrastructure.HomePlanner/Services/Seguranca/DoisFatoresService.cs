using System.Text;
using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Seguranca;
using Application.HomePlanner.Middleware;
using Application.HomePlanner.Services.Seguranca;
using Domain.HomePlanner.Models.SaaS.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Infrastructure.HomePlanner.Services.Seguranca;

public class DoisFatoresService : IDoisFatoresService
{
    private readonly UserManager<Usuario> _userManager;
    private readonly TenantContextAccessor _tenantAccessor;
    private readonly TenantContext _tenantContext;
    private readonly ILogger<DoisFatoresService> _logger;

    public DoisFatoresService(
        UserManager<Usuario> userManager,
        TenantContextAccessor tenantAccessor,
        TenantContext tenantContext,
        ILogger<DoisFatoresService> logger)
    {
        _userManager = userManager;
        _tenantAccessor = tenantAccessor;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<bool> EstaAtivoAsync(CancellationToken ct = default)
    {
        var usuario = await ObterUsuarioAsync();
        return usuario is not null && await _userManager.GetTwoFactorEnabledAsync(usuario);
    }

    public async Task<ResultadoOperacao<ChaveAutenticadorDTO>> GerarChaveAsync(CancellationToken ct = default)
    {
        var usuario = await ObterUsuarioAsync();
        if (usuario is null)
            return ResultadoOperacao<ChaveAutenticadorDTO>.Falha("Sessão inválida. Faça login novamente.");

        var chave = await _userManager.GetAuthenticatorKeyAsync(usuario);
        if (string.IsNullOrEmpty(chave))
        {
            await _userManager.ResetAuthenticatorKeyAsync(usuario);
            chave = await _userManager.GetAuthenticatorKeyAsync(usuario);
        }

        return ResultadoOperacao<ChaveAutenticadorDTO>.Ok(new ChaveAutenticadorDTO
        {
            ChaveRaw       = chave!,
            ChaveFormatada = FormatarChave(chave!),
            Email          = usuario.Email ?? string.Empty,
        });
    }

    public async Task<ResultadoOperacao<IReadOnlyList<string>>> VerificarEAtivarAsync(
        string codigo, CancellationToken ct = default)
    {
        var usuario = await ObterUsuarioAsync();
        if (usuario is null)
            return ResultadoOperacao<IReadOnlyList<string>>.Falha("Sessão inválida. Faça login novamente.");

        var codigoLimpo = (codigo ?? string.Empty).Replace(" ", string.Empty).Replace("-", string.Empty);
        var valido = await _userManager.VerifyTwoFactorTokenAsync(
            usuario, _userManager.Options.Tokens.AuthenticatorTokenProvider, codigoLimpo);

        if (!valido)
            return ResultadoOperacao<IReadOnlyList<string>>.Falha(
                "Código inválido. Verifique o app autenticador e tente novamente.");

        await _userManager.SetTwoFactorEnabledAsync(usuario, true);
        var codigos = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(usuario, 8);

        _logger.LogInformation("2FA ativado para usuário {UsuarioId}.", usuario.Id);
        return ResultadoOperacao<IReadOnlyList<string>>.Ok(codigos?.ToList() ?? []);
    }

    public async Task<ResultadoOperacao<IReadOnlyList<string>>> GerarNovosCodigosRecuperacaoAsync(
        CancellationToken ct = default)
    {
        var usuario = await ObterUsuarioAsync();
        if (usuario is null)
            return ResultadoOperacao<IReadOnlyList<string>>.Falha("Sessão inválida. Faça login novamente.");

        if (!await _userManager.GetTwoFactorEnabledAsync(usuario))
            return ResultadoOperacao<IReadOnlyList<string>>.Falha("Ative a verificação em duas etapas primeiro.");

        var codigos = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(usuario, 8);
        return ResultadoOperacao<IReadOnlyList<string>>.Ok(codigos?.ToList() ?? []);
    }

    public async Task<ResultadoOperacao> DesativarAsync(CancellationToken ct = default)
    {
        var usuario = await ObterUsuarioAsync();
        if (usuario is null)
            return ResultadoOperacao.Falha("Sessão inválida. Faça login novamente.");

        await _userManager.SetTwoFactorEnabledAsync(usuario, false);
        await _userManager.ResetAuthenticatorKeyAsync(usuario);

        _logger.LogInformation("2FA desativado para usuário {UsuarioId}.", usuario.Id);
        return ResultadoOperacao.Ok();
    }

    private async Task<Usuario?> ObterUsuarioAsync()
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        var userId = _tenantContext.UsuarioId;
        return string.IsNullOrEmpty(userId) ? null : await _userManager.FindByIdAsync(userId);
    }

    private static string FormatarChave(string chave)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < chave.Length; i++)
        {
            if (i > 0 && i % 4 == 0) sb.Append(' ');
            sb.Append(char.ToUpperInvariant(chave[i]));
        }
        return sb.ToString();
    }
}
