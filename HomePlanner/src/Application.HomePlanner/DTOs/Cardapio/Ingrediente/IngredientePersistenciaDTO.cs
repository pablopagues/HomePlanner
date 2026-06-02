namespace Application.HomePlanner.DTOs.Cardapio.Ingrediente;

public class IngredientePersistenciaDTO
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Categoria { get; set; }
    public int? UnidadeMedidaPadraoId { get; set; }
}
