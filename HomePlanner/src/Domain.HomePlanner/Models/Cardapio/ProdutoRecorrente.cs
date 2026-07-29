using Domain.HomePlanner.Models.SaaS.Interfaces;

namespace Domain.HomePlanner.Models.Cardapio;

/// <summary>
/// Produto que a família costuma comprar toda semana (ex.: "Leite", "Pão").
/// Catálogo curado e persistente do tenant — desacoplado da semana, como <see cref="ListaCompra"/>.
/// Gerenciado só pelos pais (Owner/Membro). Ao ser selecionado na tela /compras vira um
/// <see cref="PedidoCompra"/> da semana atual, herdando a loja (<see cref="ListaId"/>) e a quantidade.
/// </summary>
public class ProdutoRecorrente : ITenantEntity, IDeletableEntity, IAuditable
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }

    /// <summary>Descrição do produto (ex.: "Leite").</summary>
    public string Descricao { get; set; } = string.Empty;

    /// <summary>Quantidade padrão sugerida em texto livre (ex.: "2 L"). Opcional.</summary>
    public string? Quantidade { get; set; }

    /// <summary>Loja onde o produto é comprado, escolhida no cadastro (null = balde "Geral").</summary>
    public int? ListaId { get; set; }

    /// <summary>Inativo = não aparece na lista de seleção, mas fica guardado no catálogo.</summary>
    public bool Ativo { get; set; } = true;

    /// <summary>Ordem de exibição entre os recorrentes do tenant.</summary>
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

    // Navigation
    public ListaCompra? Lista { get; set; }
}
