using Application.HomePlanner.DTOs.Cardapio.Receita;
using Application.HomePlanner.Services.Cardapio;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace HomePlanner.BlazorServer.Api;

/// <summary>Receitas da família (CRUD + busca).</summary>
public class ReceitasController : ApiControllerBase
{
    private readonly IReceitaService _receitas;

    public ReceitasController(IReceitaService receitas) => _receitas = receitas;

    /// <summary>Lista paginada de receitas, com busca textual opcional.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] ReceitaFiltroDTO filtro, CancellationToken ct)
        => Ok(await _receitas.ListarAsync(filtro, ct));

    /// <summary>Detalhe completo de uma receita (ingredientes + componentes).</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obter(int id, CancellationToken ct)
        => Responder(await _receitas.ObterAsync(id, ct));

    /// <summary>Cria ou atualiza uma receita (Id=0 cria). Devolve o Id salvo.</summary>
    [HttpPost]
    public async Task<IActionResult> Salvar([FromBody] ReceitaPersistenciaDTO dto, CancellationToken ct)
        => Responder(await _receitas.SalvarAsync(dto, ct));

    /// <summary>Duplica uma receita existente. Devolve o Id da cópia.</summary>
    [HttpPost("{id:int}/duplicar")]
    public async Task<IActionResult> Duplicar(int id, CancellationToken ct)
        => Responder(await _receitas.DuplicarAsync(id, ct));

    /// <summary>Remove (soft-delete) uma receita.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deletar(int id, CancellationToken ct)
        => Responder(await _receitas.DeletarAsync(id, ct));

    /// <summary>Autocomplete de receitas por texto (para seleção de componentes).</summary>
    [HttpGet("autocomplete")]
    public async Task<IActionResult> AutoComplete([FromQuery] string texto, [FromQuery] int limite = 10, [FromQuery] int? excluirId = null, CancellationToken ct = default)
        => Ok(await _receitas.BuscarAutoCompleteAsync(texto, limite, excluirId, ct));

    /// <summary>Expande os componentes informados (cada um nas suas porções) em ingredientes consolidados.</summary>
    [HttpPost("expandir-componentes")]
    public async Task<IActionResult> ExpandirComponentes([FromBody] IReadOnlyList<ReceitaComponentePersistenciaDTO> componentes, CancellationToken ct)
        => Ok(await _receitas.ExpandirComponentesAsync(componentes, ct));

    /// <summary>Conteúdo binário da foto de uma receita (autorizado por JWT).</summary>
    [HttpGet("{id:int}/foto")]
    public async Task<IActionResult> Foto(int id, CancellationToken ct)
    {
        var foto = await _receitas.ObterFotoAsync(id, ct);
        if (foto is null) return NotFound();
        return File(foto.Conteudo, foto.ContentType, lastModified: null,
            entityTag: new EntityTagHeaderValue($"\"{foto.Versao}\""));
    }
}
