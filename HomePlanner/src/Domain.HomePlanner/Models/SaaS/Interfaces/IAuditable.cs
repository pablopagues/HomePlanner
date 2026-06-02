namespace Domain.HomePlanner.Models.SaaS.Interfaces;

public interface IAuditable
{
    DateTime DataCriacao { get; set; }
    DateTime? DataModificacao { get; set; }
    string? CriadoPor { get; set; }
    string? ModificadoPor { get; set; }
}
