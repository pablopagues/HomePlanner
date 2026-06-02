using Domain.HomePlanner.Models.Enums;

namespace Domain.HomePlanner.Models.Cardapio;

public class UnidadeMedida
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string ChaveTraducao { get; set; } = string.Empty;
    public TipoUnidadeMedida Tipo { get; set; }
    public decimal FatorParaBase { get; set; }
    public bool IsAtivo { get; set; } = true;
}
