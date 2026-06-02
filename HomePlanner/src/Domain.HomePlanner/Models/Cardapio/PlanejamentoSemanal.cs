using Domain.HomePlanner.Models.SaaS.Interfaces;

namespace Domain.HomePlanner.Models.Cardapio;

public class PlanejamentoSemanal : ITenantEntity, IDeletableEntity, IAuditable
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public DateOnly DataInicio { get; set; }
    public string? Nome { get; set; }
    public int? ModeloSemanaOrigemId { get; set; }

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
    public ModeloSemana? ModeloSemanaOrigem { get; set; }
    public ICollection<RefeicaoDia> RefeicoesDia { get; set; } = [];
}
