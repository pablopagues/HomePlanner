using Domain.HomePlanner.Models.Enums;

namespace Application.HomePlanner.DTOs.Cardapio.Cardapio;

public class RefeicaoDiaDTO
{
    public int Id { get; init; }
    public DiaSemana DiaSemana { get; init; }
    public TipoRefeicao TipoRefeicao { get; init; }
    public int? ReceitaId { get; init; }
    public string? ReceitaNome { get; init; }
    public string? ReceitaUrlImagem { get; init; }
    public int? PorcoesDesejadas { get; init; }
    public string? Observacao { get; init; }
}
