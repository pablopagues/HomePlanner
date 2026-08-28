using Application.HomePlanner.Common;

namespace Application.HomePlanner.DTOs.Auth;

public class RegistroResultadoDTO
{
    public bool Sucesso { get; init; }
    public Guid TenantId { get; init; }
    public string UsuarioId { get; init; } = string.Empty;

    /// <summary>Mesmo formato de <see cref="ResultadoOperacao"/>: código + texto padrão.</summary>
    public IReadOnlyList<ErroOperacao> Erros { get; init; } = [];

    public static RegistroResultadoDTO Ok(Guid tenantId, string usuarioId)
        => new() { Sucesso = true, TenantId = tenantId, UsuarioId = usuarioId };

    public static RegistroResultadoDTO Falha(params ErroOperacao[] erros)
        => new() { Sucesso = false, Erros = erros };

    /// <summary>Texto solto — mensagens do Identity e pontos ainda não migrados.</summary>
    public static RegistroResultadoDTO Falha(params string[] erros)
        => new() { Sucesso = false, Erros = erros.Select(ErroOperacao.Externa).ToArray() };
}
