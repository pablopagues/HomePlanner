using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace HomePlanner.BlazorServer.Services;

/// <summary>
/// IRequestCultureProvider que lê o cookie .HomePlanner.Lang e resolve a cultura das páginas.
/// Como sempre devolve um resultado, ele encerra a cadeia — é o que impede o Accept-Language
/// do navegador de sobrescrever a escolha explícita do usuário na web.
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

        var cookie = httpContext.Request.Cookies[PublicLanguageService.CookieName];

        var result = cookie switch
        {
            "en" => new ProviderCultureResult("en", "en"),
            "es" => new ProviderCultureResult("es", "es"),
            "fr" => new ProviderCultureResult("fr", "fr"),
            _    => new ProviderCultureResult("pt-BR", "pt-BR"),
        };

        return Task.FromResult<ProviderCultureResult?>(result);
    }
}
