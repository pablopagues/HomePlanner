namespace Application.HomePlanner.DTOs.Cardapio.Receita;

/// <summary>
/// Linha de ingrediente resultante da expansão de uma receita (própria + componentes),
/// já escalada para as porções alvo. Usada pela lista de compras e pela prévia.
/// </summary>
public class IngredienteExpandidoDTO
{
    public int IngredienteId { get; init; }
    public string NomeIngrediente { get; init; } = string.Empty;
    public decimal Quantidade { get; init; }
    public int UnidadeMedidaId { get; init; }
    public string CodigoUnidade { get; init; } = string.Empty;
    public string NomeUnidade { get; init; } = string.Empty;
}
