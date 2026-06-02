namespace Domain.HomePlanner.Models.SaaS.Tenancy;

public class TenantDadosCanada
{
    public Guid TenantId { get; set; }
    public string? Province { get; set; }
    public Tenant Tenant { get; set; } = null!;
}
