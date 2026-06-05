using Domain.HomePlanner.Models.SaaS.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Domain.HomePlanner.Models.SaaS.Identity;

public class Usuario : IdentityUser<string>, ITenantEntity, IDeletableEntity
{
    public Guid TenantId { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public DateTime DataCriacao { get; set; }
    public DateTime? UltimoLogin { get; set; }

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
