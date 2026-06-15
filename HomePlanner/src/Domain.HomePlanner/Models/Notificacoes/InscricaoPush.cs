using Domain.HomePlanner.Models.SaaS.Identity;
using Domain.HomePlanner.Models.SaaS.Interfaces;

namespace Domain.HomePlanner.Models.Notificacoes;

/// <summary>
/// Assinatura (push subscription) de um navegador/dispositivo para receber notificações Web Push.
/// Um usuário pode ter várias — uma por navegador/aparelho onde ativou.
/// </summary>
public class InscricaoPush : ITenantEntity, IDeletableEntity
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }

    /// <summary>Usuário dono desta assinatura (FK para Usuario).</summary>
    public string UsuarioId { get; set; } = string.Empty;

    /// <summary>URL única do serviço de push do navegador (FCM/APNs/etc.). É o "endereço de entrega".</summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>Chave pública do cliente (p256dh, Base64Url) usada para criptografar o payload.</summary>
    public string P256dh { get; set; } = string.Empty;

    /// <summary>Segredo de autenticação do cliente (auth, Base64Url).</summary>
    public string Auth { get; set; } = string.Empty;

    /// <summary>User-Agent de quem assinou — só para diagnóstico ("Chrome no Android").</summary>
    public string? UserAgent { get; set; }

    public DateTime DataCriacao { get; set; }
    public DateTime? UltimoEnvioEm { get; set; }

    // IDeletableEntity
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedByUsuarioId { get; set; }

    // Navigation
    public Usuario? Usuario { get; set; }
}
