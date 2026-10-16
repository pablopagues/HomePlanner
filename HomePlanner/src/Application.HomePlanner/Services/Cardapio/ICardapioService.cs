using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Cardapio.Cardapio;

namespace Application.HomePlanner.Services.Cardapio;

public interface ICardapioService
{
    Task<ResultadoOperacao<CardapioSemanaDTO>> ObterSemanaAsync(DateOnly dataInicio, CancellationToken ct = default);
    Task<ResultadoOperacao<CardapioSemanaDTO>> ObterOuCriarSemanaAsync(DateOnly dataInicio, CancellationToken ct = default);
    Task<ResultadoOperacao> DefinirRefeicaoAsync(DefinirRefeicaoCommand cmd, CancellationToken ct = default);
    /// <summary>Move (ou troca, se o destino estiver ocupado) o prato entre dois slots da semana.</summary>
    Task<ResultadoOperacao> MoverRefeicaoAsync(MoverRefeicaoCommand cmd, CancellationToken ct = default);
    /// <summary>Upsert de vários slots da semana numa única gravação (ex.: cardápio gerado por IA).</summary>
    Task<ResultadoOperacao> DefinirRefeicoesEmLoteAsync(
        DateOnly dataInicio, IReadOnlyList<DefinirRefeicaoCommand> refeicoes, CancellationToken ct = default);
    Task<ResultadoOperacao> LimparSlotAsync(int planejamentoId, int refeicaoDiaId, CancellationToken ct = default);
    Task<ResultadoOperacao<CardapioSemanaDTO>> CopiarSemanaAsync(DateOnly dataInicioOrigem, DateOnly dataInicioDestino, CancellationToken ct = default);
}
