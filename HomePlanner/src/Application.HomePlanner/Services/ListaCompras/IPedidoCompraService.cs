using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.ListaCompras;
using Application.HomePlanner.DTOs.Planner;

namespace Application.HomePlanner.Services.ListaCompras;

public interface IPedidoCompraService
{
    /// <summary>
    /// Lista os pedidos avulsos da semana agrupados por membro (categorias).
    /// Todos os papéis veem todos os pedidos; PodeEditar reflete quem pode alterar cada item.
    /// </summary>
    Task<IReadOnlyList<PedidoMembroGrupoDTO>> ListarDaSemanaAsync(
        DateOnly dataInicio, CancellationToken ct = default);

    Task<ResultadoOperacao<int>> AdicionarAsync(PedidoCompraPersistenciaDTO dto, CancellationToken ct = default);
    Task<ResultadoOperacao> MarcarCompradoAsync(int id, bool comprado, CancellationToken ct = default);
    Task<ResultadoOperacao> DeletarAsync(int id, CancellationToken ct = default);

    /// <summary>Verdadeiro quando o usuário atual só pode alterar os próprios pedidos (papel Filho).</summary>
    bool UsuarioRestritoAsProprias { get; }

    Task<IReadOnlyList<MembroFamiliaDTO>> ListarMembrosFamiliaAsync(CancellationToken ct = default);
}
