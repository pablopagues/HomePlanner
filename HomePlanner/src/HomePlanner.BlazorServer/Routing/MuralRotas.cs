namespace HomePlanner.BlazorServer.Routing;

/// <summary>
/// Allow-list de rotas acessíveis no Modo Mural. Inclui as variantes de idioma
/// (PT/EN/ES) do <see cref="UrlMap"/>. Tudo que não estiver aqui é bloqueado pelo
/// guarda de navegação do MainLayout — vale para clique no menu, botão voltar e
/// URL digitada na mão dentro do circuito.
/// </summary>
public static class MuralRotas
{
    // Primeiro segmento do path (sem barra) das telas liberadas, em todos os idiomas.
    private static readonly HashSet<string> _permitidas = new(StringComparer.OrdinalIgnoreCase)
    {
        "planner", "tasks", "tareas",      // Tarefas
        "calendario", "calendar",          // Calendário
        "cardapio", "menu",                // Menu da semana
        "compras", "shopping",             // Lista de compras
    };

    /// <summary>True se o path relativo (ex.: "compras/2026-01-01") é liberado no Mural.</summary>
    public static bool Permitida(string path)
        => _permitidas.Contains(PrimeiroSegmento(path));

    private static string PrimeiroSegmento(string path)
    {
        var p = path.Split('?')[0].Split('#')[0].Trim('/');
        if (p.Length == 0) return string.Empty;
        var idx = p.IndexOf('/');
        return idx < 0 ? p : p[..idx];
    }
}
