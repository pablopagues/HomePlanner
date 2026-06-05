namespace Application.HomePlanner.DTOs.Perfil;

/// <summary>Conteúdo bruto da foto de perfil para ser servido por um endpoint.</summary>
public class FotoUsuarioDTO
{
    public byte[] Conteudo { get; init; } = [];
    public string ContentType { get; init; } = "application/octet-stream";

    /// <summary>Token de versão (muda a cada upload) — usado para cache/cache-busting.</summary>
    public string Versao { get; init; } = string.Empty;
}
