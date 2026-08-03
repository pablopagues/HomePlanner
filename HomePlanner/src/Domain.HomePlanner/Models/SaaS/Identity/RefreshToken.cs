namespace Domain.HomePlanner.Models.SaaS.Identity;

/// <summary>
/// Refresh token opaco emitido para apps mobile. Guardamos apenas o HASH do token
/// (nunca o valor em claro) e o rotacionamos a cada uso (o antigo é revogado).
///
/// Propositalmente NÃO implementa ITenantEntity: o refresh acontece antes de o
/// contexto de tenant estar hidratado, então um filtro global por TenantId
/// impediria o lookup. O TenantId fica como coluna informativa/auditoria.
/// </summary>
public class RefreshToken
{
    public long Id { get; set; }

    /// <summary>SHA-256 (Base64) do token entregue ao cliente.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public string UsuarioId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }

    public DateTime CriadoEm { get; set; }
    public DateTime ExpiraEm { get; set; }

    /// <summary>Preenchido quando o token é rotacionado ou revogado.</summary>
    public DateTime? RevogadoEm { get; set; }

    /// <summary>Identificação do dispositivo/app (opcional, para auditoria).</summary>
    public string? DispositivoInfo { get; set; }

    public bool EstaAtivo => RevogadoEm is null && DateTime.UtcNow < ExpiraEm;

    public Usuario? Usuario { get; set; }
}
