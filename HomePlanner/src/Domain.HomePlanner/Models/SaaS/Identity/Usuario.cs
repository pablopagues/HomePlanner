using Domain.HomePlanner.Models.SaaS.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Domain.HomePlanner.Models.SaaS.Identity;

public class Usuario : IdentityUser<string>, ITenantEntity, IDeletableEntity
{
    public Guid TenantId { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;

    /// <summary>Idioma preferido (pt/en/es). Usado para notificações fora de uma requisição. Null = padrão do app.</summary>
    public string? Idioma { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? UltimoLogin { get; set; }

    /// <summary>Quando o convite de definição de senha foi enviado pela última vez (UTC).
    /// Usado para exibir o histórico ao Owner e para limitar reenvios seguidos.</summary>
    public DateTime? UltimoConviteEnviadoEm { get; set; }

    // Foto de perfil (avatar) — armazenada no próprio banco
    public byte[]? Foto { get; set; }
    public string? FotoContentType { get; set; }
    public DateTime? FotoAtualizadaEm { get; set; }

    // IDeletableEntity
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedByUsuarioId { get; set; }

    // LGPD
    public DateTime? DataAceiteTermos { get; set; }
    public string? VersaoTermosAceito { get; set; }
}
