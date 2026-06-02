using Domain.HomePlanner.Models.Enums;

namespace Application.HomePlanner.DTOs.Cardapio.ModeloSemana;

public class ModeloSemanaRefeicaoDTO
{
    public int Id { get; init; }
    public DiaSemana DiaSemana { get; init; }
    public TipoRefeicao TipoRefeicao { get; init; }
    public int? ReceitaId { get; init; }
    public string? ReceitaNome { get; init; }
    public int? PorcoesDesejadas { get; init; }
    public string? Observacao { get; init; }
}
