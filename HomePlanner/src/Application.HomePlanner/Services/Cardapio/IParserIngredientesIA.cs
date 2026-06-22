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

    Task<IReadOnlyList<IngredienteImportadoDTO>?> ParsearAsync(
        IReadOnlyList<string> linhas, CancellationToken ct = default);
}
