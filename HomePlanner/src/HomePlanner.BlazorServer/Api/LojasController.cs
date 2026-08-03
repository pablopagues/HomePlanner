using Application.HomePlanner.DTOs.ListaCompras;
using Application.HomePlanner.Services.ListaCompras;
using Microsoft.AspNetCore.Mvc;

namespace HomePlanner.BlazorServer.Api;

/// <summary>Lojas/listas customizadas (Walmart, Costco, farmácia…) e remanejamento de itens entre elas.</summary>
public class LojasController : ApiControllerBase
{
    private readonly IListaCompraService _lojas;

    public LojasController(IListaCompraService lojas) => _lojas = lojas;

    /// <summary>Lojas do tenant, ordenadas.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken ct)
        => Ok(await _lojas.ListarAsync(ct));

    /// <summary>Cria uma loja. Devolve o Id criado.</summary>
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] ListaCompraPersistenciaDTO dto, CancellationToken ct)
        => Responder(await _lojas.CriarAsync(dto, ct));

    /// <summary>Atualiza uma loja.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] ListaCompraPersistenciaDTO dto, CancellationToken ct)
        => Responder(await _lojas.AtualizarAsync(id, dto, ct));

    /// <summary>Exclui uma loja.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
        => Responder(await _lojas.ExcluirAsync(id, ct));

    /// <summary>Remaneja um item do cardápio para uma loja (listaId null = "Geral"), gravando a preferência.</summary>
    [HttpPost("atribuir-item")]
    public async Task<IActionResult> AtribuirItem([FromQuery] int ingredienteId, [FromQuery] int? listaId, CancellationToken ct)
        => Responder(await _lojas.AtribuirItemCardapioAsync(ingredienteId, listaId, ct));

    /// <summary>Remaneja o pedido de um membro para uma loja (listaId null = "Geral").</summary>
    [HttpPost("atribuir-pedido")]
    public async Task<IActionResult> AtribuirPedido([FromQuery] int pedidoId, [FromQuery] int? listaId, CancellationToken ct)
        => Responder(await _lojas.AtribuirPedidoAsync(pedidoId, listaId, ct));
}
