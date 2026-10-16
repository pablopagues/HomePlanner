using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace HomePlanner.BlazorServer.Services;

/// <summary>
/// IRequestCultureProvider que resolve a cultura das páginas pela mesma regra do
/// <see cref="PublicLanguageService"/>: cookie .HomePlanner.Lang → Accept-Language → pt-BR.
/// Como sempre devolve um resultado, ele encerra a cadeia — a escolha explícita do usuário
/// (cookie) prevalece sobre o navegador. Sem cookie (primeira visita), segue o navegador;
/// antes caía em pt-BR enquanto o seletor mostrava o idioma do navegador.
///
/// Em /api ele se abstém (devolve null) para o provider de Accept-Language assumir: o app
/// não manda cookie, e sem isso toda resposta da API sairia em pt-BR.
/// </summary>
public sealed class PublicLangCookieProvider : IRequestCultureProvider
{
    public Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        if (httpContext.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<ProviderCultureResult?>(null);

        var result = PublicLanguageService.ResolverLang(httpContext.Request) switch
        {
            "en" => new ProviderCultureResult("en", "en"),
            "es" => new ProviderCultureResult("es", "es"),
            "fr" => new ProviderCultureResult("fr", "fr"),
            _    => new ProviderCultureResult("pt-BR", "pt-BR"),
        };

        return Task.FromResult<ProviderCultureResult?>(result);
    }
}
