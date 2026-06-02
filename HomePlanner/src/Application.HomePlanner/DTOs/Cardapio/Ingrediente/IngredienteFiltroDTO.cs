namespace Application.HomePlanner.DTOs.Cardapio.Ingrediente;

public class IngredienteFiltroDTO
{
    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 20;
    public string? TextoBusca { get; set; }
    public string? Categoria { get; set; }
}
