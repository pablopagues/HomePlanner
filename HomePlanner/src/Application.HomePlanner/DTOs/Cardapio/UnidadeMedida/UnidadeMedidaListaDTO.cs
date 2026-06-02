using Domain.HomePlanner.Models.Enums;

namespace Application.HomePlanner.DTOs.Cardapio.UnidadeMedida;

public class UnidadeMedidaListaDTO
{
    public int Id { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Nome { get; init; } = string.Empty;
    public string ChaveTraducao { get; init; } = string.Empty;
    public TipoUnidadeMedida Tipo { get; init; }
    public decimal FatorParaBase { get; init; }
}
