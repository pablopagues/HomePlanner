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
}
