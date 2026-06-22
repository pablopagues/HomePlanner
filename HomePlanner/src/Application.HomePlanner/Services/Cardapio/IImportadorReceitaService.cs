using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Cardapio.Receita;

namespace Application.HomePlanner.Services.Cardapio;

public interface IImportadorReceitaService
{
    Task<ResultadoOperacao<ReceitaImportadaPreviewDTO>> ImportarDeUrlAsync(string url, CancellationToken ct = default);

    /// <summary>
    /// Parseia um texto livre de ingredientes (um por linha) em itens estruturados,
    /// detectando quantidade, unidade, opcionalidade e nome. Não acessa o banco.
    /// Versão síncrona via regex (PT/EN) — usada como fallback.
    /// </summary>
    IReadOnlyList<IngredienteImportadoDTO> ParsearTexto(string? texto);

    /// <summary>
    /// Igual ao <see cref="ParsearTexto"/>, mas usa a IA (Claude) quando habilitada,
    /// funcionando em qualquer idioma; cai no regex se a IA falhar ou estiver desligada.
    /// </summary>
    /// <param name="idiomaAlvo">Se informado ("pt"/"en"/"es"), traduz os ingredientes para esse idioma.</param>
    Task<IReadOnlyList<IngredienteImportadoDTO>> ParsearTextoAsync(
        string? texto, string? idiomaAlvo = null, CancellationToken ct = default);
}
