namespace Application.HomePlanner.DTOs.Cardapio.Receita;

/// <summary>Conteúdo bruto da foto de uma receita, para ser servido por um endpoint.</summary>
public class ReceitaFotoDTO
{
    public byte[] Conteudo { get; init; } = [];
    public string ContentType { get; init; } = "application/octet-stream";

    /// <summary>Token de versão (muda a cada upload) — usado para ETag/cache-busting.</summary>
    public string Versao { get; init; } = string.Empty;
}
