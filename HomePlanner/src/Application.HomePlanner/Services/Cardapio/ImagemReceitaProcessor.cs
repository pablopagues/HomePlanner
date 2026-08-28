using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Cardapio.Receita;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace Application.HomePlanner.Services.Cardapio;

/// <summary>
/// Processamento de foto de receita com SixLabors.ImageSharp (mesmo pacote já usado
/// no EstoqueSorveteria para logo).
///
/// Pipeline:
///   1. Valida MIME (jpeg, png, webp) e tamanho de entrada (anti-"zip bomb").
///   2. Decodifica.
///   3. Redimensiona para caber em 800×800 mantendo proporção (Mode.Max) — só reduz,
///      nunca amplia imagem menor.
///   4. Re-encoda como PNG (se tem transparência) ou JPEG qualidade 80.
///   5. Valida o tamanho final.
/// </summary>
public class ImagemReceitaProcessor : IImagemReceitaProcessor
{
    private const int LadoMaximo = 800;
    private const int TamanhoMaxEntradaBytes = 8 * 1024 * 1024;   // 8 MB de entrada
    private const int TamanhoMaxSaidaBytes = 400 * 1024;          // 400 KB de saída
    private const int JpegQuality = 80;

    private static readonly string[] MimeTypesAceitos =
        { "image/jpeg", "image/jpg", "image/png", "image/webp" };

    private readonly ILogger<ImagemReceitaProcessor> _logger;

    public ImagemReceitaProcessor(ILogger<ImagemReceitaProcessor> logger) => _logger = logger;

    public long TamanhoMaximoEntradaBytes => TamanhoMaxEntradaBytes;

    public async Task<ResultadoOperacao<ImagemProcessadaDTO>> ProcessarAsync(
        byte[] bytesOriginais, string contentType, CancellationToken ct = default)
    {
        if (bytesOriginais is null || bytesOriginais.Length == 0)
            return ResultadoOperacao<ImagemProcessadaDTO>.Falha(ErrosApp.ArquivoVazio);

        if (bytesOriginais.Length > TamanhoMaxEntradaBytes)
            return ResultadoOperacao<ImagemProcessadaDTO>.Falha(ErrosApp.ImagemEntradaMuitoGrande(TamanhoMaxEntradaBytes / (1024 * 1024)));

        var mime = (contentType ?? string.Empty).Trim().ToLowerInvariant();
        if (!MimeTypesAceitos.Contains(mime))
            return ResultadoOperacao<ImagemProcessadaDTO>.Falha(ErrosApp.ImagemFormatoInvalido);

        try
        {
            using var input = new MemoryStream(bytesOriginais);
            using var imagem = await Image.LoadAsync(input, ct);

            // PNG/WebP podem ter transparência — preservamos re-encodando como PNG.
            var temAlpha = mime is "image/png" or "image/webp";

            imagem.Mutate(ctx => ctx.Resize(new ResizeOptions
            {
                Size = new Size(LadoMaximo, LadoMaximo),
                Mode = ResizeMode.Max,          // cabe dentro do quadrado, mantém proporção
                Sampler = KnownResamplers.Lanczos3,
            }));

            using var output = new MemoryStream();
            string mimeFinal;
            if (temAlpha)
            {
                await imagem.SaveAsync(output, new PngEncoder
                {
                    CompressionLevel = PngCompressionLevel.BestCompression,
                }, ct);
                mimeFinal = "image/png";
            }
            else
            {
                await imagem.SaveAsync(output, new JpegEncoder { Quality = JpegQuality }, ct);
                mimeFinal = "image/jpeg";
            }

            var bytesFinais = output.ToArray();
            if (bytesFinais.Length > TamanhoMaxSaidaBytes)
            {
                _logger.LogWarning(
                    "Foto de receita processada ainda excede o limite: {Bytes} bytes (max {Max}).",
                    bytesFinais.Length, TamanhoMaxSaidaBytes);
                return ResultadoOperacao<ImagemProcessadaDTO>.Falha(ErrosApp.ImagemFinalAcimaDoLimite(bytesFinais.Length / 1024));
            }

            return ResultadoOperacao<ImagemProcessadaDTO>.Ok(new ImagemProcessadaDTO
            {
                Conteudo = bytesFinais,
                ContentType = mimeFinal,
            });
        }
        catch (UnknownImageFormatException)
        {
            return ResultadoOperacao<ImagemProcessadaDTO>.Falha(ErrosApp.ImagemNaoLegivel);
        }
        catch (InvalidImageContentException ex)
        {
            _logger.LogWarning(ex, "Imagem de receita com conteúdo inválido.");
            return ResultadoOperacao<ImagemProcessadaDTO>.Falha(ErrosApp.ImagemCorrompida);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao processar foto de receita.");
            return ResultadoOperacao<ImagemProcessadaDTO>.Falha(ErrosApp.ImagemErroProcessar);
        }
    }
}
