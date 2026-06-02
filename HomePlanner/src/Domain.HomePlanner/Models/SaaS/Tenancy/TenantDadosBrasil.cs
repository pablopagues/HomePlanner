namespace Domain.HomePlanner.Models.SaaS.Tenancy;

public class TenantDadosBrasil
{
    public Guid TenantId { get; set; }
    public string? Cpf { get; set; }
    public Tenant Tenant { get; set; } = null!;
}
