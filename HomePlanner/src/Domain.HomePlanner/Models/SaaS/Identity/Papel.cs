using Domain.HomePlanner.Models.Enums;
using Domain.HomePlanner.Models.SaaS.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Domain.HomePlanner.Models.SaaS.Identity;

public class Papel : IdentityRole<string>, ITenantEntity
{
    public Guid TenantId { get; set; }
    public PapelUsuario Tipo { get; set; }
    public string NomeAmigavel { get; set; } = string.Empty;
}
