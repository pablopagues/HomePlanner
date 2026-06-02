using Domain.HomePlanner.Models.SaaS.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Application.HomePlanner.Middleware;

public class TenantUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<Usuario, Papel>
{
    public TenantUserClaimsPrincipalFactory(
        UserManager<Usuario> userManager,
        RoleManager<Papel> roleManager,
        IOptions<IdentityOptions> options)
        : base(userManager, roleManager, options) { }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(Usuario user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        // Adiciona tenant_id
        if (user.TenantId != Guid.Empty)
            identity.AddClaim(new Claim("tenant_id", user.TenantId.ToString()));

        // Substitui Name pelo NomeCompleto (não o email)
        var existingName = identity.FindFirst(ClaimTypes.Name);
        if (existingName != null) identity.RemoveClaim(existingName);
        identity.AddClaim(new Claim(ClaimTypes.Name, user.NomeCompleto));

        // Troca o valor dos claims de papel pelo nome amigável (Owner/Membro/Filho)
        // — os papéis são gravados como "Owner_{tenantId}" para unicidade global.
        var roleClaims = identity.FindAll(identity.RoleClaimType).ToList();
        foreach (var rc in roleClaims)
        {
            var amigavel = rc.Value.Split('_')[0];
            if (amigavel != rc.Value)
            {
                identity.RemoveClaim(rc);
                identity.AddClaim(new Claim(identity.RoleClaimType, amigavel));
            }
        }

        return identity;
    }
}
