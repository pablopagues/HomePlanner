using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.ListaCompras;

namespace Application.HomePlanner.Services.ListaCompras;

public interface IListaCompraService
{
    /// <summary>Listas/lojas do tenant, ordenadas.</summary>
    Task<IReadOnlyList<ListaCompraDTO>> ListarAsync(CancellationToken ct = default);

    Task<ResultadoOperacao<int>> CriarAsync(ListaCompraPersistenciaDTO dto, CancellationToken ct = default);
    Task<ResultadoOperacao> AtualizarAsync(int id, ListaCompraPersistenciaDTO dto, CancellationToken ct = default);
    Task<ResultadoOperacao> ExcluirAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Remaneja um item do cardápio para uma loja (null = "Geral"). Grava a preferência
    /// do ingrediente, de modo que nas próximas semanas o item já apareça na loja certa.
    /// </summary>
    Task<ResultadoOperacao> AtribuirItemCardapioAsync(int ingredienteId, int? listaId, CancellationToken ct = default);

    /// <summary>Remaneja o pedido de um membro para uma loja (null = "Geral").</summary>
    Task<ResultadoOperacao> AtribuirPedidoAsync(int pedidoId, int? listaId, CancellationToken ct = default);

    /// <summary>Verdadeiro quando o usuário atual pode criar/editar/excluir listas (Owner/Membro).</summary>
    bool PodeGerenciar { get; }
}
