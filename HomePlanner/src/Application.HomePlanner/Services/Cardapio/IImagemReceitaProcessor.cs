using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Cardapio.Receita;

namespace Application.HomePlanner.Services.Cardapio;

/// <summary>
/// Recebe os bytes brutos de uma imagem enviada pelo usuário, valida tipo/tamanho,
/// redimensiona mantendo proporção e re-encoda comprimido — devolvendo os bytes
/// prontos para gravar na receita. Uma foto por receita (o upload substitui a anterior).
/// </summary>
public interface IImagemReceitaProcessor
{
    /// <summary>Tamanho máximo do arquivo de entrada aceito (antes do processamento).</summary>
    long TamanhoMaximoEntradaBytes { get; }

    Task<ResultadoOperacao<ImagemProcessadaDTO>> ProcessarAsync(
        byte[] bytesOriginais, string contentType, CancellationToken ct = default);
}
