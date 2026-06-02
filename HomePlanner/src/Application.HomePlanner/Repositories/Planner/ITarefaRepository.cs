using Application.HomePlanner.DTOs.Planner;
using Domain.HomePlanner.Models.Planner;

namespace Application.HomePlanner.Repositories.Planner;

public interface ITarefaRepository
{
    Task<IReadOnlyList<TarefaListaDTO>> ListarAsync(TarefaFiltroDTO filtro, CancellationToken ct = default);
    Task<int> ContarAsync(TarefaFiltroDTO filtro, CancellationToken ct = default);
    Task<Tarefa?> ObterEntidadeAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<MembroFamiliaDTO>> ListarMembrosFamiliaAsync(CancellationToken ct = default);
    Task AdicionarAsync(Tarefa entidade, CancellationToken ct = default);
    Task<int> SalvarAsync(CancellationToken ct = default);
}
