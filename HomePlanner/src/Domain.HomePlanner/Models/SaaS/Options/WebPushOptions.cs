namespace Domain.HomePlanner.Models.SaaS.Options;

public class WebPushOptions
{
    public const string SectionName = "WebPush";

    /// <summary>Liga/desliga o envio de notificações push (sem chaves VAPID, fica desabilitado).</summary>
    public bool IsEnabled { get; set; }

    /// <summary>Chave pública VAPID (Base64Url). Vai para o navegador na hora de assinar.</summary>
    public string PublicKey { get; set; } = string.Empty;

    /// <summary>Chave privada VAPID (Base64Url). Assina os envios no servidor — guardar em user-secrets.</summary>
    public string PrivateKey { get; set; } = string.Empty;

    /// <summary>Identificação do remetente exigida pelo protocolo (mailto: ou URL https).</summary>
    public string Subject { get; set; } = "mailto:support@siderisx.ca";

    /// <summary>Só está utilizável quando ligado E com o par de chaves preenchido.</summary>
    public bool EstaConfigurado =>
        IsEnabled && !string.IsNullOrWhiteSpace(PublicKey) && !string.IsNullOrWhiteSpace(PrivateKey);
}
