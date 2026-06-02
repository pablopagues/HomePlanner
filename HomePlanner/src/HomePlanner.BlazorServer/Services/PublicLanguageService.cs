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
        _lang = "pt";

        var ctx = accessor.HttpContext;
        if (ctx is null) return;

        // Prioridade 1: cookie
        if (ctx.Request.Cookies.TryGetValue(CookieName, out var cookie)
            && IsLangValida(cookie))
        {
            _lang = cookie;
            return;
        }

        // Prioridade 2: Accept-Language do browser
        var acceptLang = ctx.Request.Headers.AcceptLanguage.ToString();
        if (!string.IsNullOrEmpty(acceptLang))
        {
            var primary = acceptLang.Split(',')[0].Split(';')[0].Trim().ToLowerInvariant();
            if (primary.StartsWith("en")) _lang = "en";
            else if (primary.StartsWith("es")) _lang = "es";
        }
    }

    public string Lang => _lang;
    public bool IsPortugues => _lang == "pt";
    public bool IsIngles    => _lang == "en";
    public bool IsEspanhol  => _lang == "es";

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
        _    => new CultureInfo("pt-BR"),
    };

    public static bool IsLangValida(string? lang)
        => lang is "pt" or "en" or "es";
}
