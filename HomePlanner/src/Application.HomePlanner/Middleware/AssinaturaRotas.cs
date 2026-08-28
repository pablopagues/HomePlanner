namespace Application.HomePlanner.Middleware;

/// <summary>
/// Allow-list das rotas que continuam acessíveis quando a assinatura está bloqueada:
/// a tela de planos, o perfil e as configurações da conta (para o usuário exportar os
/// dados ou encerrar a conta) e as páginas públicas. Tudo o mais é barrado.
///
/// Fonte única para os dois guardas: o <see cref="AssinaturaRequiredMiddleware"/>
/// (carga de página, URL digitada, chamadas da API) e o guarda de navegação do
/// MainLayout (cliques dentro do circuito Blazor, que não passam pelo middleware).
/// </summary>
public static class AssinaturaRotas
{
    /// <summary>Rota para onde o Owner é mandado — é lá que ele escolhe um plano.</summary>
    public const string DestinoOwner = "/assinatura";

    /// <summary>Aviso para quem não pode contratar (Membro/Filho): fale com o responsável.</summary>
    public const string DestinoMembro = "/assinatura-expirada";

    // Primeiro segmento do path (sem barra) das telas liberadas, em todos os idiomas.
    private static readonly HashSet<string> _liberadas = new(StringComparer.OrdinalIgnoreCase)
    {
        // Assinatura — onde o bloqueio se resolve
        "assinatura", "subscription", "suscripcion", "abonnement",
        "assinatura-expirada", "subscription-expired", "suscripcion-expirada", "abonnement-expire",

        // Conta do usuário e da família: sair, exportar dados, encerrar a conta
        "perfil", "configuracoes", "empresa",
        "profile", "settings", "company",
        "profil", "configuracion",

        // Páginas públicas e institucionais
        "en", "es", "fr",
        "privacidade", "privacy", "privacidad", "confidentialite",
        "termos", "terms", "terminos", "conditions",
        "contato", "contact", "contacto", "nous-contacter",
        "quem-somos", "about", "acerca", "a-propos",

        // Infraestrutura do app (login/logout, troca de idioma, circuito Blazor)
        "identity", "onboarding", "set-lang", "health", "error",
    };

    // Segundo segmento de /api/{recurso} liberado para o app mobile.
    private static readonly HashSet<string> _apisLiberadas = new(StringComparer.OrdinalIgnoreCase)
    {
        "auth", "registro", "onboarding", "doisfatores",   // entrar na conta
        "assinatura", "webhook",                            // ver o plano / Stripe
        "perfil", "configuracao", "empresa",                // conta e configurações
        "dispositivos", "feedback",                         // push e suporte
    };

    /// <summary>True se o path relativo continua acessível com a assinatura bloqueada.</summary>
    public static bool Liberada(string path)
    {
        var p = path.Split('?')[0].Split('#')[0].Trim('/');
        if (p.Length == 0) return true; // raiz (landing page)

        var idx = p.IndexOf('/');
        var primeiro = idx < 0 ? p : p[..idx];

        // Recursos internos do Blazor (_blazor, _framework, _content): nunca bloquear,
        // senão o circuito não sobe e o usuário fica sem nem ver o aviso.
        if (primeiro.StartsWith('_')) return true;

        if (primeiro.Equals("api", StringComparison.OrdinalIgnoreCase))
        {
            var resto = idx < 0 ? string.Empty : p[(idx + 1)..];
            var idx2 = resto.IndexOf('/');
            var recurso = idx2 < 0 ? resto : resto[..idx2];
            return _apisLiberadas.Contains(recurso);
        }

        return _liberadas.Contains(primeiro);
    }
}
