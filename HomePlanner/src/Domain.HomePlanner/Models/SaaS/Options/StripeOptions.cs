using Domain.HomePlanner.Models.Enums;

namespace Domain.HomePlanner.Models.SaaS.Options;

public class StripeOptions
{
    public const string SectionName = "Stripe";

    public bool IsEnabled { get; set; }
    public string SecretKey { get; set; } = string.Empty;
    public string PublishableKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;

    public StripePriceMap Prices { get; set; } = new();
    public StripePriceMap PricesCAD { get; set; } = new();

    public StripeValorExibicaoMap ValoresBRL { get; set; } = new();
    public StripeValorExibicaoMap ValoresCAD { get; set; } = new();

    public string SuccessUrl { get; set; } = "/assinatura?status=success";
    public string CancelUrl { get; set; } = "/assinatura?status=cancel";

    public string PriceIdParaPlano(PlanoAssinatura plano, string paisId = Paises.Brasil)
    {
        var map = paisId == Paises.Canada ? PricesCAD : Prices;
        return plano switch
        {
            PlanoAssinatura.StandardMensal => map.StandardMensal,
            PlanoAssinatura.StandardAnual  => map.StandardAnual,
            PlanoAssinatura.ProMensal      => map.ProMensal,
            PlanoAssinatura.ProAnual       => map.ProAnual,
            _                              => string.Empty
        };
    }

    public string ValorExibicaoParaPlano(PlanoAssinatura plano, string paisId = Paises.Brasil)
    {
        var map = paisId == Paises.Canada ? ValoresCAD : ValoresBRL;
        return plano switch
        {
            PlanoAssinatura.StandardMensal => map.StandardMensal,
            PlanoAssinatura.StandardAnual  => map.StandardAnual,
            PlanoAssinatura.ProMensal      => map.ProMensal,
            PlanoAssinatura.ProAnual       => map.ProAnual,
            _                              => "—"
        };
    }

    public PlanoAssinatura? PlanoParaPriceId(string priceId)
    {
        if (priceId == Prices.StandardMensal || priceId == PricesCAD.StandardMensal) return PlanoAssinatura.StandardMensal;
        if (priceId == Prices.StandardAnual  || priceId == PricesCAD.StandardAnual)  return PlanoAssinatura.StandardAnual;
        if (priceId == Prices.ProMensal      || priceId == PricesCAD.ProMensal)      return PlanoAssinatura.ProMensal;
        if (priceId == Prices.ProAnual       || priceId == PricesCAD.ProAnual)       return PlanoAssinatura.ProAnual;
        return null;
    }
}

public class StripePriceMap
{
    public string StandardMensal { get; set; } = string.Empty;
    public string StandardAnual  { get; set; } = string.Empty;
    public string ProMensal      { get; set; } = string.Empty;
    public string ProAnual       { get; set; } = string.Empty;
}

public class StripeValorExibicaoMap
{
    public string StandardMensal { get; set; } = string.Empty;
    public string StandardAnual  { get; set; } = string.Empty;
    public string ProMensal      { get; set; } = string.Empty;
    public string ProAnual       { get; set; } = string.Empty;
}
