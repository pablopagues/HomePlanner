using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Cardapio.Receita;

namespace Application.HomePlanner.Services.Cardapio;

/// <summary>
/// Sugestões de receitas pela IA, com a cota mensal de sugestões do plano
/// (contada a cada pedido gerado com sucesso) e o throttle da IA.
/// </summary>
public interface ISugestaoReceitaService
{
    /// <param name="idioma">"pt" / "en" / "es" / "fr" — idioma das sugestões.</param>
    Task<ResultadoOperacao<IReadOnlyList<ReceitaImportadaPreviewDTO>>> SugerirAsync(
        PedidoSugestaoReceitaDTO pedido, string idioma, CancellationToken ct = default);
}
