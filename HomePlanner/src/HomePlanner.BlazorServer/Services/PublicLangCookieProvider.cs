using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace HomePlanner.BlazorServer.Services;

/// <summary>
/// IRequestCultureProvider que lê o cookie .HomePlanner.Lang e resolve a cultura.
/// É o único provider registrado — evita que Accept-Language sobrescreva a escolha do usuário.
/// </summary>
public sealed class PublicLangCookieProvider : IRequestCultureProvider
{
    public Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
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
