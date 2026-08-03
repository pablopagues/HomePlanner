using Application.HomePlanner.DTOs.Planner;
using Application.HomePlanner.Services.Planner;
using Microsoft.AspNetCore.Mvc;

namespace HomePlanner.BlazorServer.Api;

/// <summary>Tarefas do planner familiar. A visibilidade (Filho vê só as próprias) é aplicada pelo serviço.</summary>
public class PlannerController : ApiControllerBase
{
    private readonly ITarefaService _tarefas;

    public PlannerController(ITarefaService tarefas) => _tarefas = tarefas;

    /// <summary>Lista paginada de tarefas (busca/filtro por concluída/responsável).</summary>
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] TarefaFiltroDTO filtro, CancellationToken ct)
        => Ok(await _tarefas.ListarAsync(filtro, ct));

    /// <summary>Tarefas agendadas no intervalo [de, ate], para a visão de calendário.</summary>
    [HttpGet("calendario")]
    public async Task<IActionResult> Calendario([FromQuery] DateOnly de, [FromQuery] DateOnly ate, [FromQuery] string? responsavelUsuarioId = null, CancellationToken ct = default)
        => Ok(await _tarefas.ListarCalendarioAsync(de, ate, responsavelUsuarioId, ct));

    /// <summary>Membros da família (para atribuir responsável).</summary>
    [HttpGet("membros")]
    public async Task<IActionResult> Membros(CancellationToken ct)
        => Ok(await _tarefas.ListarMembrosFamiliaAsync(ct));

    /// <summary>Cria ou atualiza uma tarefa (Id=0 cria). Devolve o Id salvo.</summary>
    [HttpPost]
    public async Task<IActionResult> Salvar([FromBody] TarefaPersistenciaDTO dto, CancellationToken ct)
        => Responder(await _tarefas.SalvarAsync(dto, ct));

    /// <summary>Marca uma tarefa como concluída ou não.</summary>
    [HttpPost("{id:int}/concluir")]
    public async Task<IActionResult> Concluir(int id, [FromQuery] bool concluida = true, CancellationToken ct = default)
        => Responder(await _tarefas.ConcluirAsync(id, concluida, ct));

    /// <summary>Remove (soft-delete) uma tarefa.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deletar(int id, CancellationToken ct)
        => Responder(await _tarefas.DeletarAsync(id, ct));
}
