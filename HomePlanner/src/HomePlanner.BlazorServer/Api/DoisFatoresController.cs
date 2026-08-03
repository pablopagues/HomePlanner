using Application.HomePlanner.Services.Seguranca;
using Microsoft.AspNetCore.Mvc;

namespace HomePlanner.BlazorServer.Api;

/// <summary>Gestão do 2FA (app autenticador) do usuário logado.</summary>
[Route("api/2fa")]
public class DoisFatoresController : ApiControllerBase
{
    private readonly IDoisFatoresService _doisFatores;

    public DoisFatoresController(IDoisFatoresService doisFatores) => _doisFatores = doisFatores;

    /// <summary>Indica se o 2FA está ativo para o usuário atual.</summary>
    [HttpGet("status")]
    public async Task<IActionResult> Status(CancellationToken ct)
        => Ok(new { ativo = await _doisFatores.EstaAtivoAsync(ct) });

    /// <summary>Gera/recupera a chave do autenticador (para exibir QR code/chave manual).</summary>
    [HttpPost("chave")]
    public async Task<IActionResult> GerarChave(CancellationToken ct)
        => Responder(await _doisFatores.GerarChaveAsync(ct));

    /// <summary>Verifica o código TOTP, ativa o 2FA e devolve os códigos de recuperação.</summary>
    [HttpPost("ativar")]
    public async Task<IActionResult> Ativar([FromBody] CodigoRequest req, CancellationToken ct)
        => Responder(await _doisFatores.VerificarEAtivarAsync(req.Codigo, ct));

    /// <summary>Gera um novo conjunto de códigos de recuperação (invalida os anteriores).</summary>
    [HttpPost("novos-codigos-recuperacao")]
    public async Task<IActionResult> NovosCodigos(CancellationToken ct)
        => Responder(await _doisFatores.GerarNovosCodigosRecuperacaoAsync(ct));

    /// <summary>Desativa o 2FA e zera a chave do autenticador.</summary>
    [HttpPost("desativar")]
    public async Task<IActionResult> Desativar(CancellationToken ct)
        => Responder(await _doisFatores.DesativarAsync(ct));

    public record CodigoRequest(string Codigo);
}
