using Domain.HomePlanner.Models.SaaS.Interfaces;

namespace Domain.HomePlanner.Models.Cardapio;

public class Ingrediente : ITenantEntity, IDeletableEntity, IAuditable
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string NomeNormalizado { get; set; } = string.Empty;
    public string? Categoria { get; set; }
    public int? UnidadeMedidaPadraoId { get; set; }

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
    public UnidadeMedida? UnidadeMedidaPadrao { get; set; }
    public ICollection<ReceitaIngrediente> ReceitasIngredientes { get; set; } = [];
}
