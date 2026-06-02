namespace Application.HomePlanner.DTOs.Cardapio.Ingrediente;

public class IngredienteDetalheDTO
{
    public int Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string? Categoria { get; init; }
    public int? UnidadeMedidaPadraoId { get; init; }
    public string? CodigoUnidadePadrao { get; init; }
    public string? NomeUnidadePadrao { get; init; }
    public DateTime DataCriacao { get; init; }
}
