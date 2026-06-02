using Domain.HomePlanner.Models.Enums;

namespace Application.HomePlanner.DTOs.ListaCompras;

public class ItemComprasDTO
{
    public int IngredienteId { get; init; }
    public string NomeIngrediente { get; init; } = string.Empty;
    public string? Categoria { get; init; }
    public decimal Quantidade { get; init; }
    public string CodigoUnidade { get; init; } = string.Empty;
    public string NomeUnidade { get; init; } = string.Empty;
    public TipoUnidadeMedida Tipo { get; init; }

    public string QuantidadeFormatada =>
        Quantidade == Math.Floor(Quantidade)
            ? $"{(int)Quantidade} {CodigoUnidade}"
            : $"{Quantidade:G} {CodigoUnidade}";
}
