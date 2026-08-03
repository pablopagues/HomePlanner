using Application.HomePlanner.DTOs.ListaCompras;
using Application.HomePlanner.Services.ListaCompras;
using Microsoft.AspNetCore.Mvc;

namespace HomePlanner.BlazorServer.Api;

/// <summary>
/// Lista de compras da semana: itens agregados do cardápio (com marcações compartilhadas)
/// e pedidos avulsos por membro.
/// </summary>
public class ComprasController : ApiControllerBase
{
    private readonly IListaComprasService _lista;
    private readonly IPedidoCompraService _pedidos;

    public ComprasController(IListaComprasService lista, IPedidoCompraService pedidos)
    {
        _lista = lista;
        _pedidos = pedidos;
    }

    // ── Itens agregados do cardápio ──────────────────────────────────────────

    /// <summary>Lista de compras agregada das receitas da semana (segunda-feira em yyyy-MM-dd).</summary>
    [HttpGet("semana/{dataInicio}")]
    public async Task<IActionResult> DaSemana(DateOnly dataInicio, CancellationToken ct)
        => Responder(await _lista.CalcularDaSemanaAsync(dataInicio, ct));

    /// <summary>Mapa IngredienteId → comprado/já tenho (estado compartilhado da semana).</summary>
    [HttpGet("semana/{dataInicio}/marcacoes")]
    public async Task<IActionResult> Marcacoes(DateOnly dataInicio, CancellationToken ct)
        => Ok(await _lista.ObterMarcacoesSemanaAsync(dataInicio, ct));

    /// <summary>Marca/desmarca um ingrediente da semana.</summary>
    [HttpPost("semana/{dataInicio}/marcar")]
    public async Task<IActionResult> Marcar(DateOnly dataInicio, [FromQuery] int ingredienteId, [FromQuery] bool comprado, CancellationToken ct)
        => Responder(await _lista.MarcarItemAsync(dataInicio, ingredienteId, comprado, ct));

    /// <summary>Desmarca todos os itens da semana.</summary>
    [HttpPost("semana/{dataInicio}/limpar-marcacoes")]
    public async Task<IActionResult> LimparMarcacoes(DateOnly dataInicio, CancellationToken ct)
        => Responder(await _lista.LimparMarcacoesSemanaAsync(dataInicio, ct));

    // ── Pedidos avulsos por membro ───────────────────────────────────────────

    /// <summary>Pedidos avulsos da semana agrupados por membro.</summary>
    [HttpGet("semana/{dataInicio}/pedidos")]
    public async Task<IActionResult> Pedidos(DateOnly dataInicio, CancellationToken ct)
        => Ok(await _pedidos.ListarDaSemanaAsync(dataInicio, ct));

    /// <summary>Adiciona um pedido avulso. Devolve o Id criado.</summary>
    [HttpPost("pedidos")]
    public async Task<IActionResult> AdicionarPedido([FromBody] PedidoCompraPersistenciaDTO dto, CancellationToken ct)
        => Responder(await _pedidos.AdicionarAsync(dto, ct));

    /// <summary>Marca/desmarca um pedido como comprado.</summary>
    [HttpPost("pedidos/{id:int}/comprado")]
    public async Task<IActionResult> MarcarPedido(int id, [FromQuery] bool comprado, CancellationToken ct)
        => Responder(await _pedidos.MarcarCompradoAsync(id, comprado, ct));

    /// <summary>Remove um pedido avulso.</summary>
    [HttpDelete("pedidos/{id:int}")]
    public async Task<IActionResult> DeletarPedido(int id, CancellationToken ct)
        => Responder(await _pedidos.DeletarAsync(id, ct));
}
