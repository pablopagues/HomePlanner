using Domain.HomePlanner.Models.Enums;

namespace Application.HomePlanner.DTOs.Cardapio.Cardapio;

public class DefinirRefeicaoCommand
{
    public DateOnly DataInicio { get; set; }
    public DiaSemana DiaSemana { get; set; }
    public TipoRefeicao TipoRefeicao { get; set; }
    public int? ReceitaId { get; set; }
    public int? PorcoesDesejadas { get; set; }
    public string? Observacao { get; set; }
}
