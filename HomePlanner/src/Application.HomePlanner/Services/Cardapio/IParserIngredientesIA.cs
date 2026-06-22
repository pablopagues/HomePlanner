using Application.HomePlanner.DTOs.Cardapio.Receita;

namespace Application.HomePlanner.Services.Cardapio;

/// <summary>
/// Parsing estruturado de linhas de ingrediente usando IA (Claude), funcionando
/// em qualquer idioma. Retorna <c>null</c> quando a IA está desabilitada ou
/// falha — nesse caso o chamador deve usar o parser regex como fallback.
/// </summary>
public interface IParserIngredientesIA
{
    /// <summary>true quando há chave de API configurada.</summary>
    bool Habilitado { get; }

    /// <param name="idiomaAlvo">
    /// Quando informado ("pt"/"en"/"es"), os campos <c>nome</c> e <c>preparo</c>
    /// retornam traduzidos para esse idioma (o <c>textoOriginal</c> fica intacto).
    /// </param>
    Task<IReadOnlyList<IngredienteImportadoDTO>?> ParsearAsync(
        IReadOnlyList<string> linhas, string? idiomaAlvo = null, CancellationToken ct = default);
}
