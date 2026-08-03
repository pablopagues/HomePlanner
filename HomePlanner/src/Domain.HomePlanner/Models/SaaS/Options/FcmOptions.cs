namespace Domain.HomePlanner.Models.SaaS.Options;

/// <summary>
/// Configuração do Firebase Cloud Messaging (push para os apps MAUI — Android e iOS via APNs).
/// A credencial é o JSON da conta de serviço do Firebase; guardar em user-secrets/arquivo no
/// servidor, nunca comitada. É independente do Web Push (VAPID) usado pelo site.
/// </summary>
public class FcmOptions
{
    public const string SectionName = "Fcm";

    /// <summary>Liga/desliga o push nativo (FCM).</summary>
    public bool IsEnabled { get; set; }

    /// <summary>JSON da conta de serviço (service account) — cole o conteúdo aqui OU use CredentialsPath.</summary>
    public string? CredentialsJson { get; set; }

    /// <summary>Caminho para o arquivo JSON da conta de serviço (alternativa ao CredentialsJson).</summary>
    public string? CredentialsPath { get; set; }

    /// <summary>Só utilizável quando ligado E com uma credencial (JSON inline ou arquivo).</summary>
    public bool EstaConfigurado =>
        IsEnabled && (!string.IsNullOrWhiteSpace(CredentialsJson) || !string.IsNullOrWhiteSpace(CredentialsPath));
}
