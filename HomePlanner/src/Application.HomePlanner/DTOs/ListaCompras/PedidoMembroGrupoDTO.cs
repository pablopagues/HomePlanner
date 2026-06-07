namespace Application.HomePlanner.DTOs.ListaCompras;

/// <summary>Categoria da lista de compras agrupando os pedidos de um membro.</summary>
public class PedidoMembroGrupoDTO
{
    public string? SolicitanteUsuarioId { get; init; }
    public string SolicitanteNome { get; init; } = string.Empty;
    public DateTime? SolicitanteFotoAtualizadaEm { get; init; }
    public IReadOnlyList<PedidoCompraDTO> Itens { get; init; } = [];

    public string? SolicitanteFotoVersao => SolicitanteFotoAtualizadaEm?.Ticks.ToString();

    public int TotalComprados => Itens.Count(i => i.Comprado);
}
