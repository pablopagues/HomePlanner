using Microsoft.Extensions.Localization;

namespace Application.HomePlanner.Common;

/// <summary>
/// Traduz <see cref="ErroOperacao"/> na borda de exibição. É aqui que o código vira texto —
/// nem o serviço nem a API decidem idioma.
/// </summary>
public static class TradutorErroExtensions
{
    /// <summary>
    /// Texto do erro no idioma atual. Cai no texto padrão quando o erro veio de fora
    /// (Identity, Stripe) ou quando a chave <c>Erro_{codigo}</c> ainda não existe —
    /// nunca devolve o código cru para a tela.
    /// </summary>
    public static string Traduzir(this IStringLocalizer localizador, ErroOperacao erro)
    {
        if (erro.Externo) return erro.TextoPadrao;

        var chave = $"Erro_{erro.Codigo}";
        var traduzido = localizador[chave];
        if (traduzido.ResourceNotFound) return erro.TextoPadrao;

        return erro.Args.Count == 0
            ? traduzido.Value
            : string.Format(traduzido.Value, erro.Args.ToArray());
    }

    /// <summary>Junta os erros de um resultado numa frase só, já traduzidos.</summary>
    public static string Traduzir(this IStringLocalizer localizador, IEnumerable<ErroOperacao> erros)
        => string.Join(" ", erros.Select(localizador.Traduzir));
}
