namespace Application.HomePlanner.Lgpd;

/// <summary>
/// Constantes da conformidade LGPD.
///
/// VERSIONAMENTO: incrementar <see cref="VersaoAtual"/> sempre que houver MUDANÇA
/// SIGNIFICATIVA na Política de Privacidade ou nos Termos de Uso. Isso força:
///   1. Re-aceite no próximo registro (o aceite grava esta versão no usuário).
///   2. Re-exibição do banner de cookies para visitantes que já tinham aceitado.
///   3. Auditoria correta de "qual versão o usuário aceitou".
/// </summary>
public static class LgpdConstants
{
    /// <summary>Versão atual dos termos e política. Hoje: v1.</summary>
    public const string VersaoAtual = "v1";

    /// <summary>Nome do cookie que armazena o consentimento do visitante.</summary>
    public const string CookieConsentName = "consent_lgpd";

    /// <summary>Validade do cookie de consentimento (12 meses — recomendação ANPD).</summary>
    public static readonly TimeSpan CookieConsentDuration = TimeSpan.FromDays(365);

    /// <summary>E-mail do Encarregado pelo Tratamento de Dados (DPO).</summary>
    public const string EmailDpo = "sideris.sistemas@gmail.com";

    /// <summary>Nome do controlador para exibição.</summary>
    public const string NomeControlador = "HomePlanner";

    /// <summary>Data da última atualização dos documentos (alinhar com <see cref="VersaoAtual"/>).</summary>
    public const string DataUltimaAtualizacao = "15 de junho de 2026";
}
