using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Planner;

namespace Application.HomePlanner.Services.Planner;

public interface ITarefaService
{
    Task<ResultadoListagem<TarefaListaDTO>> ListarAsync(TarefaFiltroDTO filtro, CancellationToken ct = default);
    Task<ResultadoOperacao<int>> SalvarAsync(TarefaPersistenciaDTO dto, CancellationToken ct = default);
    Task<ResultadoOperacao> ConcluirAsync(int id, bool concluida, CancellationToken ct = default);
    Task<ResultadoOperacao> DeletarAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<MembroFamiliaDTO>> ListarMembrosFamiliaAsync(CancellationToken ct = default);
}
