using Application.HomePlanner.DTOs.Cardapio.Cardapio;
using Domain.HomePlanner.Models.Cardapio;
using Domain.HomePlanner.Models.Enums;

namespace Application.HomePlanner.Repositories.Cardapio;

public interface IPlanejamentoSemanalRepository
{
    Task<PlanejamentoSemanal?> ObterPorDataInicioAsync(DateOnly dataInicio, CancellationToken ct = default);
    Task<PlanejamentoSemanal?> ObterEntidadeAsync(int id, CancellationToken ct = default);
    Task<CardapioSemanaDTO?> ObterCardapioSemanaAsync(DateOnly dataInicio, CancellationToken ct = default);
    Task<RefeicaoDia?> ObterRefeicaoDiaAsync(int planejamentoId, DiaSemana dia, TipoRefeicao tipo, CancellationToken ct = default);
    Task<RefeicaoDia?> ObterRefeicaoDiaPorIdAsync(int id, CancellationToken ct = default);
    Task AdicionarAsync(PlanejamentoSemanal entidade, CancellationToken ct = default);
    Task AdicionarRefeicaoDiaAsync(RefeicaoDia entidade, CancellationToken ct = default);
    Task<int> SalvarAsync(CancellationToken ct = default);
}
