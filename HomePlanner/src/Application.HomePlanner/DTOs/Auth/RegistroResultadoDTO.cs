namespace Application.HomePlanner.DTOs.Auth;

public class RegistroResultadoDTO
{
    public bool Sucesso { get; init; }
    public Guid TenantId { get; init; }
    public string UsuarioId { get; init; } = string.Empty;
    public IReadOnlyList<string> Erros { get; init; } = [];

    public static RegistroResultadoDTO Ok(Guid tenantId, string usuarioId)
        => new() { Sucesso = true, TenantId = tenantId, UsuarioId = usuarioId };

    public static RegistroResultadoDTO Falha(params string[] erros)
        => new() { Sucesso = false, Erros = erros };
}
