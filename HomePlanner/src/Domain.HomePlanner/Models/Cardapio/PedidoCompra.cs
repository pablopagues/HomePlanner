using Domain.HomePlanner.Models.SaaS.Identity;
using Domain.HomePlanner.Models.SaaS.Interfaces;

namespace Domain.HomePlanner.Models.Cardapio;

/// <summary>
/// Item avulso pedido por um membro da família para a lista de compras de uma semana.
/// Complementa os itens calculados a partir do cardápio (que não são persistidos).
/// A semana é identificada pela segunda-feira (<see cref="DataInicioSemana"/>), mesma
/// chave usada pela tela /compras — sem FK obrigatória para PlanejamentoSemanal.
/// </summary>
public class PedidoCompra : ITenantEntity, IDeletableEntity, IAuditable
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }

    /// <summary>Segunda-feira da semana à qual o pedido pertence.</summary>
    public DateOnly DataInicioSemana { get; set; }

    /// <summary>Descrição do item (ex.: "Shampoo").</summary>
    public string Descricao { get; set; } = string.Empty;

    /// <summary>Quantidade em texto livre (ex.: "2 frascos"). Opcional.</summary>
    public string? Quantidade { get; set; }

    /// <summary>Membro que fez o pedido (FK para Usuario).</summary>
    public string? SolicitanteUsuarioId { get; set; }

    /// <summary>Estado compartilhado: item já comprado.</summary>
    public bool Comprado { get; set; }
    public DateTime? DataCompra { get; set; }

    // IDeletableEntity
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedByUsuarioId { get; set; }

    // IAuditable
    public DateTime DataCriacao { get; set; }
    public DateTime? DataModificacao { get; set; }
    public string? CriadoPor { get; set; }
    public string? ModificadoPor { get; set; }

    // Navigation
    public Usuario? Solicitante { get; set; }
}
