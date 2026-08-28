namespace HomePlanner.BlazorServer.Routing;

/// <summary>
/// Mapeamento bidirecional de URLs por idioma (PT ↔ EN ↔ ES ↔ FR).
/// PT é o idioma canônico — as URLs PT são as chaves do mapa.
/// </summary>
public static class UrlMap
{
    // ── Páginas públicas ──────────────────────────────────────────────────────
    private static readonly Dictionary<string, (string En, string Es, string Fr)> _ptParaOutros =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["/"]              = ("/en",         "/es",          "/fr"),
            ["/privacidade"]   = ("/privacy",    "/privacidad",  "/confidentialite"),
            ["/termos"]        = ("/terms",       "/terminos",    "/conditions"),
            ["/contato"]       = ("/contact",     "/contacto",    "/nous-contacter"),
            ["/quem-somos"]    = ("/about",       "/acerca",      "/a-propos"),
            ["/assinatura"]    = ("/subscription", "/suscripcion", "/abonnement"),
            ["/assinatura-expirada"] = ("/subscription-expired", "/suscripcion-expirada", "/abonnement-expire"),
            ["/perfil"]        = ("/profile",     "/perfil",      "/profil"),
            // ── Páginas autenticadas (URL única, idioma via cookie) ──────────
            ["/dashboard"]     = ("/dashboard",   "/dashboard",   "/dashboard"),
            ["/cardapio"]      = ("/menu",        "/menu",        "/menu"),
            ["/receitas"]      = ("/recipes",     "/recetas",     "/recipes"),
            ["/modelos"]       = ("/templates",   "/plantillas",  "/templates"),
            ["/compras"]       = ("/shopping",    "/compras",     "/shopping"),
            ["/planner"]       = ("/tasks",       "/tareas",      "/tasks"),
            ["/calendario"]    = ("/calendar",    "/calendario",  "/calendar"),
            ["/configuracoes"] = ("/settings",    "/configuracion", "/settings"),
            ["/familia"]       = ("/family",      "/familia",     "/family"),
            ["/empresa"]       = ("/company",     "/empresa",     "/company"),
        };

    // Mapas inversos calculados a partir do mapa principal
    private static readonly Dictionary<string, string> _enParaPt;
    private static readonly Dictionary<string, string> _esParaPt;
    private static readonly Dictionary<string, string> _frParaPt;

    static UrlMap()
    {
        _enParaPt = _ptParaOutros
            .ToDictionary(kv => kv.Value.En, kv => kv.Key, StringComparer.OrdinalIgnoreCase);
        _esParaPt = _ptParaOutros
            .ToDictionary(kv => kv.Value.Es, kv => kv.Key, StringComparer.OrdinalIgnoreCase);
        _frParaPt = _ptParaOutros
            .ToDictionary(kv => kv.Value.Fr, kv => kv.Key, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Retorna a URL no idioma solicitado, usando PT como canônico.</summary>
    public static string Get(string lang, string ptUrl)
    {
        if (!_ptParaOutros.TryGetValue(ptUrl, out var outros)) return ptUrl;
        return lang switch
        {
            "en" => outros.En,
            "es" => outros.Es,
            "fr" => outros.Fr,
            _    => ptUrl,
        };
    }

    /// <summary>Traduz uma URL de um idioma para outro.</summary>
    public static string Translate(string currentPath, string fromLang, string toLang)
    {
        if (fromLang == toLang) return currentPath;
        var path = currentPath.Split('?')[0].Split('#')[0];

        // Converte para PT primeiro (idioma canônico)
        var ptUrl = fromLang switch
        {
            "en" => _enParaPt.TryGetValue(path, out var pt1) ? pt1 : path,
            "es" => _esParaPt.TryGetValue(path, out var pt2) ? pt2 : path,
            "fr" => _frParaPt.TryGetValue(path, out var pt3) ? pt3 : path,
            _    => path,
        };

        return Get(toLang, ptUrl);
    }
}
