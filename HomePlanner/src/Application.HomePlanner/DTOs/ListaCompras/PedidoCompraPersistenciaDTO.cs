namespace Application.HomePlanner.DTOs.ListaCompras;

/// <summary>Dados de criação de um pedido avulso na lista de compras.</summary>
public class PedidoCompraPersistenciaDTO
{
    public DateOnly DataInicioSemana { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public string? Quantidade { get; set; }

    /// <summary>
    /// Membro a quem o pedido pertence. Para papéis restritos (Filho) é ignorado
    /// e forçado ao próprio usuário; Owner/Membro podem escolher (null = si mesmo).
    /// </summary>
    public string? SolicitanteUsuarioId { get; set; }
}
