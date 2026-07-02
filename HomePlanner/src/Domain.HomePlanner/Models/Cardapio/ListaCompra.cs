using Domain.HomePlanner.Models.SaaS.Interfaces;

namespace Domain.HomePlanner.Models.Cardapio;

/// <summary>
/// Lista/loja de compras customizada do tenant (ex.: "Walmart", "Costco", "Farmácia").
/// Persistente e reutilizável entre semanas — diferente dos itens do cardápio (semanais).
/// Itens (do cardápio ou pedidos de membros) são atribuídos a uma lista para organizar
/// onde comprar; itens sem lista caem no balde "Geral".
/// </summary>
public class ListaCompra : ITenantEntity, IDeletableEntity, IAuditable
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }

    /// <summary>Nome exibido da lista/loja (ex.: "Walmart").</summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>Ícone MudBlazor opcional (ex.: "Store"). Só o nome curto, sem o namespace.</summary>
    public string? Icone { get; set; }

    /// <summary>Cor de destaque opcional (hex, ex.: "#2E7D32").</summary>
    public string? Cor { get; set; }

    /// <summary>Ordem de exibição entre as listas do tenant.</summary>
    public int Ordem { get; set; }

    // IDeletableEntity
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedByUsuarioId { get; set; }

    // IAuditable
    public DateTime DataCriacao { get; set; }
    public DateTime? DataModificacao { get; set; }
    public string? CriadoPor { get; set; }
    public string? ModificadoPor { get; set; }
}
