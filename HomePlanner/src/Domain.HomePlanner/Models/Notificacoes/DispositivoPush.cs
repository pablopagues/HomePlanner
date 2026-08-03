using Domain.HomePlanner.Models.SaaS.Identity;
using Domain.HomePlanner.Models.SaaS.Interfaces;

namespace Domain.HomePlanner.Models.Notificacoes;

/// <summary>
/// Token de registro FCM de um app nativo (MAUI). Um usuário pode ter vários — um por aparelho.
/// Diferente de <see cref="InscricaoPush"/> (Web Push/navegador): aqui guardamos o token do
/// Firebase que endereça o push ao aparelho via FCM (Android) / APNs (iOS).
/// </summary>
public class DispositivoPush : ITenantEntity, IDeletableEntity
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }

    /// <summary>Usuário dono deste aparelho (FK para Usuario).</summary>
    public string UsuarioId { get; set; } = string.Empty;

    /// <summary>Token de registro FCM do aparelho (o "endereço de entrega").</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Plataforma do aparelho ("android"/"ios") — só para diagnóstico/segmentação.</summary>
    public string? Plataforma { get; set; }

    /// <summary>Modelo/SO do aparelho — só para diagnóstico.</summary>
    public string? DispositivoInfo { get; set; }

    public DateTime DataCriacao { get; set; }
    public DateTime? UltimoEnvioEm { get; set; }

    // IDeletableEntity
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedByUsuarioId { get; set; }

    // Navigation
    public Usuario? Usuario { get; set; }
}
