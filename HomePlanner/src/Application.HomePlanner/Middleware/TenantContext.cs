namespace Application.HomePlanner.Middleware;

public class TenantContext
{
    public Guid? TenantId { get; private set; }
    public string? UsuarioId { get; private set; }
    public string UsuarioNome { get; private set; } = string.Empty;
    public bool EstaHidratado { get; private set; }

    public void Definir(Guid? tenantId, string? usuarioId, string usuarioNome = "")
    {
        TenantId = tenantId;
        UsuarioId = usuarioId;
        UsuarioNome = usuarioNome;
        EstaHidratado = true;
    }
}
