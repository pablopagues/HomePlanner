using System.Globalization;
using HomePlanner.BlazorServer.Routing;
using Microsoft.AspNetCore.Http;

namespace HomePlanner.BlazorServer.Services;

/// <summary>
/// Serviço com escopo de request/circuit que rastreia o idioma atual do usuário.
/// </summary>
public sealed class PublicLanguageService
{
    public const string CookieName = ".HomePlanner.Lang";

    private string _lang;

    public PublicLanguageService(IHttpContextAccessor accessor)
    {
        var ctx = accessor.HttpContext;
        _lang = ctx is null ? "pt" : ResolverLang(ctx.Request);
    }

    /// <summary>
    /// Regra única do idioma das páginas: cookie (escolha explícita) → Accept-Language do
    /// navegador → "pt". Usada também pelo <see cref="PublicLangCookieProvider"/>, para a
    /// cultura dos textos e o seletor de idioma nunca divergirem.
    /// </summary>
    public static string ResolverLang(HttpRequest request)
    {
        // Prioridade 1: cookie
        if (request.Cookies.TryGetValue(CookieName, out var cookie) && IsLangValida(cookie))
            return cookie!;

        // Prioridade 2: Accept-Language do browser
        var acceptLang = request.Headers.AcceptLanguage.ToString();
        if (!string.IsNullOrEmpty(acceptLang))
        {
            var primary = acceptLang.Split(',')[0].Split(';')[0].Trim().ToLowerInvariant();
            if (primary.StartsWith("en")) return "en";
            if (primary.StartsWith("es")) return "es";
            if (primary.StartsWith("fr")) return "fr";
        }
        return "pt";
    }

    public string Lang => _lang;
    public bool IsPortugues => _lang == "pt";
    public bool IsIngles    => _lang == "en";
    public bool IsEspanhol  => _lang == "es";
    public bool IsFrances   => _lang == "fr";

    /// Retorna a URL traduzida para o idioma atual.
    public string U(string ptUrl) => UrlMap.Get(_lang, ptUrl);

    /// Reaplicar cultura no circuito Blazor Server (chamado nos layouts).
    public void SetLang(string lang)
    {
        if (!IsLangValida(lang)) return;
        _lang = lang;
        var culture = LangParaCulture(lang);
        CultureInfo.CurrentCulture   = culture;
        CultureInfo.CurrentUICulture = culture;
    }

    public static CultureInfo LangParaCulture(string lang) => lang switch
    {
        "en" => new CultureInfo("en-US"),
        "es" => new CultureInfo("es-ES"),
        "fr" => new CultureInfo("fr-FR"),
        _    => new CultureInfo("pt-BR"),
    };

    public static bool IsLangValida(string? lang)
        => lang is "pt" or "en" or "es" or "fr";
}
