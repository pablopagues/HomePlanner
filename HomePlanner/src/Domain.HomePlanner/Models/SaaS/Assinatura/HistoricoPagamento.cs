using Domain.HomePlanner.Models.Enums;
using Domain.HomePlanner.Models.SaaS.Interfaces;
using Domain.HomePlanner.Models.SaaS.Tenancy;

namespace Domain.HomePlanner.Models.SaaS.Assinatura;

public class HistoricoPagamento : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public PlanoAssinatura Plano { get; set; }
    public decimal Valor { get; set; }
    public string Moeda { get; set; } = "BRL";
    public DateTime DataPagamento { get; set; }
    public string? StripeInvoiceId { get; set; }
    public string? StripePaymentIntentId { get; set; }
    public bool Sucesso { get; set; }
    public string? MotivoFalha { get; set; }
    public Tenant Tenant { get; set; } = null!;
}
