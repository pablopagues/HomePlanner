using Application.HomePlanner.Services.Assinatura;
using Microsoft.AspNetCore.Mvc;

namespace HomePlanner.BlazorServer.Api;

/// <summary>
/// Assinatura — somente leitura no app. Contratação/upgrade acontece na versão web
/// (Apple/Google exigem In-App Purchase para venda de assinatura digital no app).
/// </summary>
public class AssinaturaController : ApiControllerBase
{
    private readonly IAssinaturaService _assinatura;

    public AssinaturaController(IAssinaturaService assinatura) => _assinatura = assinatura;

    /// <summary>Plano atual, status e limites — para o app exibir e apontar upgrades para o site.</summary>
    [HttpGet]
    public async Task<IActionResult> Minha(CancellationToken ct)
        => Ok(await _assinatura.ObterMinhaAssinaturaAsync(ct));
}
