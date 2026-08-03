using Application.HomePlanner.DTOs.Cardapio.Ingrediente;
using Application.HomePlanner.Services.Cardapio;
using Microsoft.AspNetCore.Mvc;

namespace HomePlanner.BlazorServer.Api;

/// <summary>Catálogo de ingredientes da família.</summary>
public class IngredientesController : ApiControllerBase
{
    private readonly IIngredienteService _ingredientes;

    public IngredientesController(IIngredienteService ingredientes) => _ingredientes = ingredientes;

    /// <summary>Lista paginada de ingredientes, com busca textual opcional.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] IngredienteFiltroDTO filtro, CancellationToken ct)
        => Ok(await _ingredientes.ListarAsync(filtro, ct));

    /// <summary>Detalhe de um ingrediente.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obter(int id, CancellationToken ct)
        => Responder(await _ingredientes.ObterAsync(id, ct));

    /// <summary>Cria ou atualiza um ingrediente (Id=0 cria). Devolve o Id salvo.</summary>
    [HttpPost]
    public async Task<IActionResult> Salvar([FromBody] IngredientePersistenciaDTO dto, CancellationToken ct)
        => Responder(await _ingredientes.SalvarAsync(dto, ct));

    /// <summary>Remove (soft-delete) um ingrediente.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deletar(int id, CancellationToken ct)
        => Responder(await _ingredientes.DeletarAsync(id, ct));

    /// <summary>Autocomplete de ingredientes por texto.</summary>
    [HttpGet("autocomplete")]
    public async Task<IActionResult> AutoComplete([FromQuery] string texto, [FromQuery] int limite = 10, CancellationToken ct = default)
        => Ok(await _ingredientes.BuscarAutoCompleteAsync(texto, limite, ct));

    /// <summary>Ingredientes com nome parecido (para evitar duplicatas ao cadastrar).</summary>
    [HttpGet("similares")]
    public async Task<IActionResult> Similares([FromQuery] string nome, [FromQuery] int? excluirId = null, CancellationToken ct = default)
        => Ok(await _ingredientes.DetectarSimilaresAsync(nome, excluirId, ct));

    /// <summary>Define (ou remove, com baseId nulo) o produto base de compra de um ingrediente.</summary>
    [HttpPost("{id:int}/produto-base")]
    public async Task<IActionResult> DefinirProdutoBase(int id, [FromQuery] int? baseId, CancellationToken ct)
        => Responder(await _ingredientes.DefinirProdutoBaseAsync(id, baseId, ct));
}
