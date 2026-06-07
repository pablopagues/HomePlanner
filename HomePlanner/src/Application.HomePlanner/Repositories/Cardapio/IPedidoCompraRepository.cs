using Application.HomePlanner.DTOs.ListaCompras;
using Application.HomePlanner.DTOs.Planner;
using Domain.HomePlanner.Models.Cardapio;

namespace Application.HomePlanner.Repositories.Cardapio;

public interface IPedidoCompraRepository
{
    /// <summary>Lista os pedidos da semana (ordenados por solicitante e descrição). PodeEditar não é definido aqui.</summary>
    Task<IReadOnlyList<PedidoCompraDTO>> ListarDaSemanaAsync(DateOnly dataInicio, CancellationToken ct = default);
    Task<PedidoCompra?> ObterEntidadeAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<MembroFamiliaDTO>> ListarMembrosFamiliaAsync(CancellationToken ct = default);
    Task AdicionarAsync(PedidoCompra entidade, CancellationToken ct = default);
    Task<int> SalvarAsync(CancellationToken ct = default);
}
