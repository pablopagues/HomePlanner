using Domain.HomePlanner.Models.Enums;

namespace Domain.HomePlanner.Models.Cardapio;

public class RefeicaoDia
{
    public int Id { get; set; }
    public int PlanejamentoSemanalId { get; set; }
    public DiaSemana DiaSemana { get; set; }
    public TipoRefeicao TipoRefeicao { get; set; }
    public int? ReceitaId { get; set; }
    public int? PorcoesDesejadas { get; set; }
    public string? Observacao { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation
    public PlanejamentoSemanal PlanejamentoSemanal { get; set; } = null!;
    public Receita? Receita { get; set; }
}
