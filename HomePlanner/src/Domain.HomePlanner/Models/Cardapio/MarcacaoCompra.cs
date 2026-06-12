using Domain.HomePlanner.Models.SaaS.Interfaces;

namespace Domain.HomePlanner.Models.Cardapio;

/// <summary>
/// Marcação compartilhada ("já tenho/comprado") de um item da lista de compras gerada
/// do cardápio. A chave é semana (segunda-feira) + ingrediente (produto base). Por ser
/// do tenant, toda a família enxerga o mesmo estado — diferente do antigo localStorage.
/// </summary>
public class MarcacaoCompra : ITenantEntity, IAuditable
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }

    /// <summary>Segunda-feira da semana à qual a marcação pertence.</summary>
    public DateOnly DataInicioSemana { get; set; }

    /// <summary>Ingrediente (produto base) marcado.</summary>
    public int IngredienteId { get; set; }

    public bool Comprado { get; set; }
    public DateTime? DataCompra { get; set; }

    // IAuditable
    public DateTime DataCriacao { get; set; }
    public DateTime? DataModificacao { get; set; }
    public string? CriadoPor { get; set; }
    public string? ModificadoPor { get; set; }
}
