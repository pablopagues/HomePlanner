using Application.HomePlanner.DTOs.Cardapio.ModeloSemana;
using Application.HomePlanner.Services.Cardapio;
using Microsoft.AspNetCore.Mvc;

namespace HomePlanner.BlazorServer.Api;

/// <summary>Modelos de semana (templates de cardápio reutilizáveis).</summary>
public class ModelosController : ApiControllerBase
{
    private readonly IModeloSemanaService _modelos;

    public ModelosController(IModeloSemanaService modelos) => _modelos = modelos;

    /// <summary>Lista paginada de modelos.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] ModeloSemanaFiltroDTO filtro, CancellationToken ct)
        => Ok(await _modelos.ListarAsync(filtro, ct));

    /// <summary>Detalhe de um modelo (refeições).</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obter(int id, CancellationToken ct)
        => Responder(await _modelos.ObterAsync(id, ct));

    /// <summary>Cria ou atualiza um modelo (Id=0 cria). Devolve o Id salvo.</summary>
    [HttpPost]
    public async Task<IActionResult> Salvar([FromBody] ModeloSemanaPersistenciaDTO dto, CancellationToken ct)
        => Responder(await _modelos.SalvarAsync(dto, ct));

    /// <summary>Remove um modelo.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deletar(int id, CancellationToken ct)
        => Responder(await _modelos.DeletarAsync(id, ct));

    /// <summary>Aplica um modelo à semana (segunda-feira). Devolve o cardápio resultante.</summary>
    [HttpPost("{id:int}/aplicar")]
    public async Task<IActionResult> Aplicar(int id, [FromQuery] DateOnly dataInicio, CancellationToken ct)
        => Responder(await _modelos.AplicarModeloAsync(id, dataInicio, ct));

    /// <summary>Salva o cardápio de uma semana como um novo modelo. Devolve o Id criado.</summary>
    [HttpPost("salvar-da-semana")]
    public async Task<IActionResult> SalvarDaSemana([FromQuery] DateOnly dataInicio, [FromQuery] string nome, CancellationToken ct)
        => Responder(await _modelos.SalvarPlanejamentoComoModeloAsync(dataInicio, nome, ct));
}
