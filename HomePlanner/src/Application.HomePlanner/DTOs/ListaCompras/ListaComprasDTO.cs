namespace Application.HomePlanner.DTOs.ListaCompras;

public class ListaComprasDTO
{
    public DateOnly DataInicio { get; init; }
    public int TotalReceitas { get; init; }
    public IReadOnlyList<ItemComprasDTO> Itens { get; init; } = [];

    public bool Vazia => Itens.Count == 0;
}
