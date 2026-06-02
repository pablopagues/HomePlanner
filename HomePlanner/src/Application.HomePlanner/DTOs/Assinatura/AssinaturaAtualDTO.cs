using Domain.HomePlanner.Models.Enums;

namespace Application.HomePlanner.DTOs.Assinatura;

/// <summary>Snapshot da assinatura do tenant para exibição na tela.</summary>
public class AssinaturaAtualDTO
{
    public PlanoAssinatura Plano { get; set; }
    public StatusAssinatura Status { get; set; }

    public DateTime? DataFimTrial { get; set; }
    public DateTime? DataExpiracao { get; set; }
    public DateTime? DataProximaCobranca { get; set; }
    public DateTime? DataCancelamento { get; set; }
    public bool CanceladoAoFimDoPeriodo { get; set; }

    public string? UltimoCartaoFinal { get; set; }

    /// <summary>true se há StripeCustomerId — owner pode abrir o Customer Portal.</summary>
    public bool TemAssinaturaStripe { get; set; }

    /// <summary>Limite de membros da família no plano atual.</summary>
    public int LimiteMembros { get; set; }

    /// <summary>Dias restantes no trial (null se já é paga).</summary>
    public int? DiasRestantesTrial { get; set; }

    public string Moeda { get; set; } = "BRL";
    public string SimboloMoeda { get; set; } = "R$";

    /// <summary>Valores formatados por plano, prontos para a UI.</summary>
    public Dictionary<PlanoAssinatura, string> PrecosDisplay { get; set; } = new();
}

/// <summary>Resultado de iniciar checkout/portal — URL do Stripe para redirect.</summary>
public class StripeRedirectDTO
{
    public string Url { get; set; } = string.Empty;
}
