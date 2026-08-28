using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Assinatura;
using Application.HomePlanner.Middleware;
using Application.HomePlanner.Repositories.Assinatura;
using Domain.HomePlanner.Models.Enums;
using Domain.HomePlanner.Models.SaaS.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.HomePlanner.Services.Assinatura;

public class AssinaturaService : IAssinaturaService
{
    private readonly IAssinaturaRepository _repo;
    private readonly IStripeBillingService _stripe;
    private readonly StripeOptions _stripeOptions;
    private readonly TenantContext _tenantContext;
    private readonly TenantContextAccessor _tenantAccessor;
    private readonly ILogger<AssinaturaService> _logger;

    public AssinaturaService(
        IAssinaturaRepository repo,
        IStripeBillingService stripe,
        IOptions<StripeOptions> stripeOptions,
        TenantContext tenantContext,
        TenantContextAccessor tenantAccessor,
        ILogger<AssinaturaService> logger)
    {
        _repo = repo;
        _stripe = stripe;
        _stripeOptions = stripeOptions.Value;
        _tenantContext = tenantContext;
        _tenantAccessor = tenantAccessor;
        _logger = logger;
    }

    public async Task<AssinaturaAtualDTO> ObterMinhaAssinaturaAsync(CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        var tenantId = _tenantContext.TenantId ?? Guid.Empty;

        var paisId = await ObterPaisIdAsync(tenantId, ct);
        var moeda  = MoedaDoPais(paisId);

        var a = await _repo.ObterMinhaAssinaturaAsync(ct);
        if (a is null)
        {
            return new AssinaturaAtualDTO
            {
                Plano         = PlanoAssinatura.Gratis,
                Status        = StatusAssinatura.Trial,
                LimiteMembros = LimitesPorPlano.Membros(PlanoAssinatura.Gratis),
                LimiteImportacoesReceita = LimitesPorPlano.ImportacoesReceitaMes(PlanoAssinatura.Gratis),
                Moeda         = moeda,
                SimboloMoeda  = SimboloDaMoeda(moeda),
                PrecosDisplay = MontarPrecosDisplay(paisId),
            };
        }

        var dto = new AssinaturaAtualDTO
        {
            Plano                   = a.Plano,
            Status                  = a.Status,
            DataFimTrial            = a.DataFimTrial,
            DataExpiracao           = a.DataExpiracao,
            DataProximaCobranca     = a.DataProximaCobranca,
            DataCancelamento        = a.DataCancelamento,
            CanceladoAoFimDoPeriodo = a.CanceladoAoFimDoPeriodo,
            UltimoCartaoFinal       = a.UltimoCartaoFinal,
            TemAssinaturaStripe     = !string.IsNullOrWhiteSpace(a.StripeCustomerId),
            LimiteMembros           = LimitesPorPlano.Membros(a.Plano),
            LimiteImportacoesReceita = LimitesPorPlano.ImportacoesReceitaMes(a.Plano),
            Moeda                   = moeda,
            SimboloMoeda            = SimboloDaMoeda(moeda),
            PrecosDisplay           = MontarPrecosDisplay(paisId),
        };

        // Dias restantes de trial
        if (a.Status == StatusAssinatura.Trial && a.DataFimTrial.HasValue)
        {
            var diff = (a.DataFimTrial.Value.Date - DateTime.UtcNow.Date).Days;
            dto.DiasRestantesTrial = Math.Max(0, diff);
        }

        return dto;
    }

    public async Task<ResultadoOperacao<StripeRedirectDTO>> IniciarCheckoutAsync(
        PlanoAssinatura plano, string baseUrlAplicacao, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        if (plano == PlanoAssinatura.Gratis)
            return ResultadoOperacao<StripeRedirectDTO>.Falha("O plano grátis é o trial inicial — não requer checkout.");

        if (!_stripeOptions.IsEnabled)
            return ResultadoOperacao<StripeRedirectDTO>.Falha("A integração com o Stripe não está habilitada.");

        var assinatura = await _repo.ObterMinhaAssinaturaAsync(ct);
        if (assinatura is null)
            return ResultadoOperacao<StripeRedirectDTO>.Falha("Conta sem configuração de assinatura.");

        var tenantId = _tenantContext.TenantId ?? Guid.Empty;
        var paisId   = await ObterPaisIdAsync(tenantId, ct);
        var email    = await _repo.ObterEmailTenantAsync(tenantId, ct) ?? string.Empty;

        var priceId = _stripeOptions.PriceIdParaPlano(plano, paisId);
        if (string.IsNullOrWhiteSpace(priceId))
            return ResultadoOperacao<StripeRedirectDTO>.Falha(
                $"Preço não configurado para o plano {plano} ({paisId}). Configure em Stripe:Prices.");

        try
        {
            var url = await _stripe.CriarCheckoutSessionUrlAsync(
                plano, email, assinatura.StripeCustomerId, tenantId, paisId,
                successUrl: $"{baseUrlAplicacao.TrimEnd('/')}{_stripeOptions.SuccessUrl}",
                cancelUrl:  $"{baseUrlAplicacao.TrimEnd('/')}{_stripeOptions.CancelUrl}",
                ct: ct);
            return ResultadoOperacao<StripeRedirectDTO>.Ok(new StripeRedirectDTO { Url = url });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao criar Checkout Session.");
            return ResultadoOperacao<StripeRedirectDTO>.Falha(ErrosApp.CheckoutFalhou);
        }
    }

    public async Task<ResultadoOperacao<StripeRedirectDTO>> IniciarGerenciamentoAsync(
        string baseUrlAplicacao, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        var assinatura = await _repo.ObterMinhaAssinaturaAsync(ct);
        if (assinatura is null || string.IsNullOrWhiteSpace(assinatura.StripeCustomerId))
            return ResultadoOperacao<StripeRedirectDTO>.Falha(ErrosApp.SemAssinaturaAtiva);

        try
        {
            var url = await _stripe.CriarCustomerPortalUrlAsync(
                assinatura.StripeCustomerId,
                returnUrl: $"{baseUrlAplicacao.TrimEnd('/')}/assinatura",
                ct: ct);
            return ResultadoOperacao<StripeRedirectDTO>.Ok(new StripeRedirectDTO { Url = url });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao abrir Customer Portal.");
            return ResultadoOperacao<StripeRedirectDTO>.Falha(ErrosApp.PortalFalhou);
        }
    }

    private async Task<string> ObterPaisIdAsync(Guid tenantId, CancellationToken ct)
        => await _repo.ObterPaisIdAsync(tenantId, ct) ?? Paises.Brasil;

    private static string MoedaDoPais(string paisId) => paisId switch
    {
        Paises.Canada => "CAD",
        _             => "BRL",
    };

    private static string SimboloDaMoeda(string moeda) => moeda switch
    {
        "CAD" => "CA$",
        _     => "R$",
    };

    private Dictionary<PlanoAssinatura, string> MontarPrecosDisplay(string paisId)
    {
        var planos = new[]
        {
            PlanoAssinatura.StandardMensal,
            PlanoAssinatura.StandardAnual,
            PlanoAssinatura.ProMensal,
            PlanoAssinatura.ProAnual,
        };

        var result = new Dictionary<PlanoAssinatura, string>();
        foreach (var p in planos)
        {
            var valor = _stripeOptions.ValorExibicaoParaPlano(p, paisId);
            result[p] = string.IsNullOrWhiteSpace(valor) ? "—" : valor;
        }
        return result;
    }
}
