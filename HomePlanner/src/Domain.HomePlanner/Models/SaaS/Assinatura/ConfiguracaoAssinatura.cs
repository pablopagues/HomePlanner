using Domain.HomePlanner.Models.Enums;
using Domain.HomePlanner.Models.SaaS.Interfaces;
using Domain.HomePlanner.Models.SaaS.Tenancy;

namespace Domain.HomePlanner.Models.SaaS.Assinatura;

public class ConfiguracaoAssinatura : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public PlanoAssinatura Plano { get; set; } = PlanoAssinatura.Gratis;
    public StatusAssinatura Status { get; set; } = StatusAssinatura.Trial;
    public DateTime DataInicioTrial { get; set; }
    public DateTime? DataFimTrial { get; set; }
    public DateTime? DataExpiracao { get; set; }
    public DateTime? DataProximaCobranca { get; set; }
    public DateTime? DataCancelamento { get; set; }
    public bool CanceladoAoFimDoPeriodo { get; set; }
    public string? StripeCustomerId { get; set; }
    public string? StripeSubscriptionId { get; set; }
    public string? StripePriceId { get; set; }
    public string? UltimoCartaoFinal { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataModificacao { get; set; }
    public Tenant Tenant { get; set; } = null!;
}
