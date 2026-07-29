namespace Application.HomePlanner.DTOs.Cardapio.Receita;

/// <summary>Bytes de uma imagem já redimensionada/comprimida, prontos para persistir.</summary>
public class ImagemProcessadaDTO
{
    public byte[] Conteudo { get; init; } = [];
    public string ContentType { get; init; } = "application/octet-stream";
}
