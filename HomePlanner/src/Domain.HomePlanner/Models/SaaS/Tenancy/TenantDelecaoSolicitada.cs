namespace Domain.HomePlanner.Models.SaaS.Tenancy;

public class TenantDelecaoSolicitada
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public DateTime DataSolicitacao { get; set; }
    public DateTime DataExecucaoAgendada { get; set; }
    public bool Executado { get; set; }
    public DateTime? DataExecucao { get; set; }
    public string SolicitadoPorUsuarioId { get; set; } = string.Empty;
    public Tenant Tenant { get; set; } = null!;
}
