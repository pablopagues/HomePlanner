using Application.HomePlanner.DTOs.Familia;
using Application.HomePlanner.Services.Familia;
using Domain.HomePlanner.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HomePlanner.BlazorServer.Api;

/// <summary>Gestão dos membros da família (a maioria das ações exige papel Owner — validado no serviço).</summary>
public class FamiliaController : ApiControllerBase
{
    private readonly IFamiliaService _familia;

    public FamiliaController(IFamiliaService familia) => _familia = familia;

    /// <summary>Lista os membros da família com seus papéis.</summary>
    [HttpGet("membros")]
    public async Task<IActionResult> Membros(CancellationToken ct)
        => Ok(await _familia.ListarMembrosAsync(ct));

    /// <summary>Uso de assentos: membros ativos vs. limite do plano.</summary>
    [HttpGet("resumo")]
    public async Task<IActionResult> Resumo(CancellationToken ct)
        => Ok(await _familia.ObterResumoAsync(ct));

    /// <summary>Adiciona um membro e envia o convite de definição de senha.</summary>
    [HttpPost("membros")]
    public async Task<IActionResult> Adicionar([FromBody] NovoMembroDTO dto, CancellationToken ct)
        => Responder(await _familia.AdicionarMembroAsync(dto, BaseUrl(), ct));

    /// <summary>Reenvia (regenera) o convite de senha de um membro.</summary>
    [HttpPost("membros/{usuarioId}/reenviar-convite")]
    public async Task<IActionResult> ReenviarConvite(string usuarioId, CancellationToken ct)
        => Responder(await _familia.ReenviarConviteAsync(usuarioId, BaseUrl(), ct));

    /// <summary>Edita nome e e-mail de um membro.</summary>
    [HttpPut("membros/{usuarioId}")]
    public async Task<IActionResult> Editar(string usuarioId, [FromBody] EditarMembroRequest req, CancellationToken ct)
        => Responder(await _familia.EditarMembroAsync(usuarioId, req.Nome, req.Email, ct));

    /// <summary>Altera o papel de um membro (Owner/Membro/Filho).</summary>
    [HttpPost("membros/{usuarioId}/papel")]
    public async Task<IActionResult> AlterarPapel(string usuarioId, [FromBody] AlterarPapelRequest req, CancellationToken ct)
        => Responder(await _familia.AlterarPapelAsync(usuarioId, req.NovoPapel, ct));

    /// <summary>Ativa ou desativa (bloqueia login) um membro.</summary>
    [HttpPost("membros/{usuarioId}/ativo")]
    public async Task<IActionResult> DefinirAtivo(string usuarioId, [FromQuery] bool ativo, CancellationToken ct)
        => Responder(await _familia.DefinirAtivoAsync(usuarioId, ativo, ct));

    /// <summary>Remove (soft-delete) um membro, liberando o assento do plano.</summary>
    [HttpDelete("membros/{usuarioId}")]
    public async Task<IActionResult> Remover(string usuarioId, CancellationToken ct)
        => Responder(await _familia.RemoverMembroAsync(usuarioId, ct));

    private string BaseUrl() => $"{Request.Scheme}://{Request.Host}";

    public record EditarMembroRequest(string Nome, string Email);
    public record AlterarPapelRequest(PapelUsuario NovoPapel);
}
