namespace Application.HomePlanner.DTOs.Cardapio.Ingrediente;

public class IngredienteListaDTO
{
    public int Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string? Categoria { get; init; }
    public int? UnidadeMedidaPadraoId { get; init; }
    public string? CodigoUnidadePadrao { get; init; }

    /// <summary>Receitas que usam o ingrediente (só preenchido na listagem do catálogo).</summary>
    public int TotalReceitas { get; init; }
}
