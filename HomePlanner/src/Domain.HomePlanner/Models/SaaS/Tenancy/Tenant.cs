namespace Domain.HomePlanner.Models.SaaS.Tenancy;

public class Tenant
{
    public Guid Id { get; set; }
    public string NomeResponsavel { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PaisId { get; set; }
    public bool Ativo { get; set; } = true;
    public bool OnboardingCompleto { get; set; }
    public DateTime DataCriacao { get; set; }
    public string OwnerUsuarioId { get; set; } = string.Empty;
}
