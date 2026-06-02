using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Cardapio.Cardapio;

namespace Application.HomePlanner.Services.Cardapio;

public interface ICardapioService
{
    Task<ResultadoOperacao<CardapioSemanaDTO>> ObterSemanaAsync(DateOnly dataInicio, CancellationToken ct = default);
    Task<ResultadoOperacao<CardapioSemanaDTO>> ObterOuCriarSemanaAsync(DateOnly dataInicio, CancellationToken ct = default);
    Task<ResultadoOperacao> DefinirRefeicaoAsync(DefinirRefeicaoCommand cmd, CancellationToken ct = default);
    Task<ResultadoOperacao> LimparSlotAsync(int planejamentoId, int refeicaoDiaId, CancellationToken ct = default);
    Task<ResultadoOperacao<CardapioSemanaDTO>> CopiarSemanaAsync(DateOnly dataInicioOrigem, DateOnly dataInicioDestino, CancellationToken ct = default);
}
