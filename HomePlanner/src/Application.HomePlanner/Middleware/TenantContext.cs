using System.Security.Claims;

namespace Application.HomePlanner.Middleware;

public class TenantContext
{
    public Guid? TenantId { get; private set; }
    public string? UsuarioId { get; private set; }
    public string UsuarioNome { get; private set; } = string.Empty;
    public bool EstaHidratado { get; private set; }

    /// <summary>
    /// Verdadeiro quando o usuário só pode enxergar os próprios registros (papel Filho).
    /// Owner e Membro (pai/mãe) têm visão de toda a família.
    /// </summary>
    public bool RestritoAsProprias { get; private set; }

    /// <summary>Verdadeiro quando o usuário é o administrador (papel Owner) da família.</summary>
    public bool EhOwner { get; private set; }

    public void Definir(Guid? tenantId, string? usuarioId, string usuarioNome = "",
        bool restritoAsProprias = false, bool ehOwner = false)
    {
        TenantId = tenantId;
        UsuarioId = usuarioId;
        UsuarioNome = usuarioNome;
        RestritoAsProprias = restritoAsProprias;
        EhOwner = ehOwner;
        EstaHidratado = true;
    }

    /// <summary>
    /// Hidrata a partir das claims do usuário autenticado (tenant_id, id, nome, papéis).
    /// Não faz nada se o principal não estiver autenticado ou não tiver tenant_id válido.
    /// </summary>
    public void DefinirAPartirDe(ClaimsPrincipal? usuario)
    {
        if (usuario?.Identity?.IsAuthenticated != true) return;
        if (!Guid.TryParse(usuario.FindFirstValue("tenant_id"), out var tenantId)) return;

        // Filho (e qualquer papel que não seja Owner/Membro) só enxerga os próprios registros.
        var ehOwner = usuario.IsInRole("Owner");
        var restrito = !ehOwner && !usuario.IsInRole("Membro");

        Definir(tenantId,
            usuario.FindFirstValue(ClaimTypes.NameIdentifier),
            usuario.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
            restrito, ehOwner);
    }
}
