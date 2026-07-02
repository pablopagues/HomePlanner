namespace Application.HomePlanner.DTOs.ListaCompras;

/// <summary>Item de pedido avulso (de um membro) na lista de compras.</summary>
public class PedidoCompraDTO
{
    public int Id { get; init; }
    public string Descricao { get; init; } = string.Empty;
    public string? Quantidade { get; init; }
    public string? SolicitanteUsuarioId { get; init; }
    public string SolicitanteNome { get; init; } = string.Empty;
    public DateTime? SolicitanteFotoAtualizadaEm { get; init; }
    public bool Comprado { get; init; }

    /// <summary>Lista/loja para onde o pedido foi remanejado (null = balde "Geral").</summary>
    public int? ListaId { get; set; }

    /// <summary>Usuário atual pode editar/remover este pedido (Owner/Membro ou o próprio solicitante).</summary>
    public bool PodeEditar { get; set; }

    public string? SolicitanteFotoVersao => SolicitanteFotoAtualizadaEm?.Ticks.ToString();
}
