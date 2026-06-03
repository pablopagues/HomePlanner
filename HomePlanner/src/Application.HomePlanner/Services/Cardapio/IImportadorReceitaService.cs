using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Cardapio.Receita;

namespace Application.HomePlanner.Services.Cardapio;

public interface IImportadorReceitaService
{
    Task<ResultadoOperacao<ReceitaImportadaPreviewDTO>> ImportarDeUrlAsync(string url, CancellationToken ct = default);

    /// <summary>
    /// Parseia um texto livre de ingredientes (um por linha) em itens estruturados,
    /// detectando quantidade, unidade, opcionalidade e nome. Não acessa o banco.
    /// </summary>
    IReadOnlyList<IngredienteImportadoDTO> ParsearTexto(string? texto);
}
