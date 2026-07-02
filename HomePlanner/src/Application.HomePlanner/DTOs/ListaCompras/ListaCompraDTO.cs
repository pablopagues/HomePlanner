namespace Application.HomePlanner.DTOs.ListaCompras;

/// <summary>Lista/loja de compras customizada (ex.: "Walmart", "Costco", "Farmácia").</summary>
public class ListaCompraDTO
{
    public int Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string? Icone { get; init; }
    public string? Cor { get; init; }
    public int Ordem { get; init; }
}
