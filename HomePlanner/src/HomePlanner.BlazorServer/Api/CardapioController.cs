using Application.HomePlanner.DTOs.Cardapio.Cardapio;
using Application.HomePlanner.Services.Cardapio;
using Microsoft.AspNetCore.Mvc;

namespace HomePlanner.BlazorServer.Api;

/// <summary>
/// Controller piloto: prova que a lógica de negócio (ICardapioService) serve a API
/// sem qualquer alteração — o tenant é resolvido a partir dos claims do JWT.
/// </summary>
public class CardapioController : ApiControllerBase
{
    private readonly ICardapioService _cardapio;

    public CardapioController(ICardapioService cardapio) => _cardapio = cardapio;

    /// <summary>Semana do cardápio a partir da segunda-feira informada (yyyy-MM-dd), criando se não existir.</summary>
    [HttpGet("semana/{dataInicio}")]
    public async Task<IActionResult> ObterSemana(DateOnly dataInicio, CancellationToken ct)
        => Responder(await _cardapio.ObterOuCriarSemanaAsync(dataInicio, ct));

    /// <summary>Define (ou limpa) a receita de um slot dia/refeição.</summary>
    [HttpPost("refeicao")]
    public async Task<IActionResult> DefinirRefeicao([FromBody] DefinirRefeicaoCommand cmd, CancellationToken ct)
        => Responder(await _cardapio.DefinirRefeicaoAsync(cmd, ct));

    /// <summary>Copia o cardápio de uma semana para outra.</summary>
    [HttpPost("copiar")]
    public async Task<IActionResult> CopiarSemana([FromQuery] DateOnly origem, [FromQuery] DateOnly destino, CancellationToken ct)
        => Responder(await _cardapio.CopiarSemanaAsync(origem, destino, ct));

    /// <summary>Limpa um slot (remove a receita de um dia/refeição específico).</summary>
    [HttpDelete("slot")]
    public async Task<IActionResult> LimparSlot([FromQuery] int planejamentoId, [FromQuery] int refeicaoDiaId, CancellationToken ct)
        => Responder(await _cardapio.LimparSlotAsync(planejamentoId, refeicaoDiaId, ct));
}
