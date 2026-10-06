using System.Security.Claims;
using Application.HomePlanner.Middleware;
using Xunit;

namespace Tests.HomePlanner;

public class TenantContextTests
{
    private static readonly Guid Tenant = Guid.NewGuid();

    private static ClaimsPrincipal Usuario(params string[] papeis)
    {
        var claims = new List<Claim>
        {
            new("tenant_id", Tenant.ToString()),
            new(ClaimTypes.NameIdentifier, "u1"),
            new(ClaimTypes.Name, "Ana"),
        };
        claims.AddRange(papeis.Select(p => new Claim(ClaimTypes.Role, p)));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies"));
    }

    [Fact]
    public void Owner_VeFamiliaInteira()
    {
        var ctx = new TenantContext();
        ctx.DefinirAPartirDe(Usuario("Owner"));

        Assert.True(ctx.EstaHidratado);
        Assert.Equal(Tenant, ctx.TenantId);
        Assert.Equal("u1", ctx.UsuarioId);
        Assert.Equal("Ana", ctx.UsuarioNome);
        Assert.True(ctx.EhOwner);
        Assert.False(ctx.RestritoAsProprias);
    }

    [Fact]
    public void Membro_NaoEhRestrito()
    {
        var ctx = new TenantContext();
        ctx.DefinirAPartirDe(Usuario("Membro"));

        Assert.False(ctx.EhOwner);
        Assert.False(ctx.RestritoAsProprias);
    }

    [Fact]
    public void Filho_SoVeOsProprios()
    {
        var ctx = new TenantContext();
        ctx.DefinirAPartirDe(Usuario("Filho"));

        Assert.True(ctx.RestritoAsProprias);
    }

    [Fact]
    public void NaoAutenticado_NaoHidrata()
    {
        var ctx = new TenantContext();
        ctx.DefinirAPartirDe(new ClaimsPrincipal(new ClaimsIdentity()));
        ctx.DefinirAPartirDe(null);

        Assert.False(ctx.EstaHidratado);
    }

    [Fact]
    public void SemTenantValido_NaoHidrata()
    {
        var ctx = new TenantContext();
        ctx.DefinirAPartirDe(new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim("tenant_id", "lixo") }, "Cookies")));

        Assert.False(ctx.EstaHidratado);
    }
}
