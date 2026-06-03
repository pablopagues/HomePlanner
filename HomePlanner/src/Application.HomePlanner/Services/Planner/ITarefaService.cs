using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Planner;

namespace Application.HomePlanner.Services.Planner;

public interface ITarefaService
{
    Task<ResultadoListagem<TarefaListaDTO>> ListarAsync(TarefaFiltroDTO filtro, CancellationToken ct = default);

    /// <summary>
    /// Lista tarefas agendadas no intervalo [de, ate] para o calendário.
    /// Owner/Membro veem toda a família; demais papéis (Filho) só as próprias.
    /// <paramref name="responsavelUsuarioId"/> filtra por membro (ignorado para papéis restritos).
    /// </summary>
    Task<IReadOnlyList<TarefaListaDTO>> ListarCalendarioAsync(
        DateOnly de, DateOnly ate, string? responsavelUsuarioId = null, CancellationToken ct = default);

    /// <summary>Verdadeiro quando o usuário atual só pode ver as próprias tarefas (papel Filho).</summary>
    bool UsuarioRestritoAsProprias { get; }
    Task<ResultadoOperacao<int>> SalvarAsync(TarefaPersistenciaDTO dto, CancellationToken ct = default);
    Task<ResultadoOperacao> ConcluirAsync(int id, bool concluida, CancellationToken ct = default);
    Task<ResultadoOperacao> DeletarAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<MembroFamiliaDTO>> ListarMembrosFamiliaAsync(CancellationToken ct = default);
}
