using Application.HomePlanner.DTOs.ListaCompras;
using Application.HomePlanner.Services.ListaCompras;
using Microsoft.AspNetCore.Mvc;

namespace HomePlanner.BlazorServer.Api;

/// <summary>Catálogo de produtos recorrentes (curado por Owner/Membro) e envio para a lista da semana.</summary>
public class ProdutosRecorrentesController : ApiControllerBase
{
    private readonly IProdutoRecorrenteService _recorrentes;

    public ProdutosRecorrentesController(IProdutoRecorrenteService recorrentes) => _recorrentes = recorrentes;

    /// <summary>Produtos do catálogo (apenasAtivos=true filtra os inativos).</summary>
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] bool apenasAtivos = false, CancellationToken ct = default)
        => Ok(await _recorrentes.ListarAsync(apenasAtivos, ct));

    /// <summary>Cria um produto recorrente. Devolve o Id criado.</summary>
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] ProdutoRecorrentePersistenciaDTO dto, CancellationToken ct)
        => Responder(await _recorrentes.CriarAsync(dto, ct));

    /// <summary>Atualiza um produto recorrente.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] ProdutoRecorrentePersistenciaDTO dto, CancellationToken ct)
        => Responder(await _recorrentes.AtualizarAsync(id, dto, ct));

    /// <summary>Exclui um produto recorrente.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
        => Responder(await _recorrentes.ExcluirAsync(id, ct));

    /// <summary>Descrições de pedidos passados ainda fora do catálogo (para importar em lote).</summary>
    [HttpGet("sugestoes-historico")]
    public async Task<IActionResult> SugestoesHistorico(CancellationToken ct)
        => Ok(await _recorrentes.ListarSugestoesHistoricoAsync(ct));

    /// <summary>Cria recorrentes em lote a partir de descrições do histórico. Devolve a quantidade criada.</summary>
    [HttpPost("importar-historico")]
    public async Task<IActionResult> ImportarHistorico([FromBody] IReadOnlyCollection<string> descricoes, CancellationToken ct)
        => Responder(await _recorrentes.ImportarDoHistoricoAsync(descricoes, ct));

    /// <summary>Envia os recorrentes escolhidos para a lista da semana informada.</summary>
    [HttpPost("adicionar-semana")]
    public async Task<IActionResult> AdicionarSemana([FromBody] IReadOnlyCollection<int> produtoIds, [FromQuery] DateOnly dataInicio, CancellationToken ct)
        => Responder(await _recorrentes.AdicionarASemanaAsync(produtoIds, dataInicio, ct));
}
