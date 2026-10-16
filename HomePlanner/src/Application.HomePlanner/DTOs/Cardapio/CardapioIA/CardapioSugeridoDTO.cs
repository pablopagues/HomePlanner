using Domain.HomePlanner.Models.Enums;

namespace Application.HomePlanner.DTOs.Cardapio.CardapioIA;

/// <summary>
/// Prévia do cardápio gerado pela IA (ainda não gravado). Mutável: a família pode trocar
/// ou esvaziar horários antes de aceitar.
/// </summary>
public class CardapioSugeridoDTO
{
    public DateOnly DataInicio { get; set; }
    public List<RefeicaoSugeridaDTO> Refeicoes { get; set; } = [];

    /// <summary>Comentário geral da IA sobre o cardápio (no idioma da interface).</summary>
    public string? Observacoes { get; set; }

    public int HorariosVazios => Refeicoes.Count(r => r.ReceitaId is null);
}

public class RefeicaoSugeridaDTO
{
    public DiaSemana DiaSemana { get; set; }
    public TipoRefeicao TipoRefeicao { get; set; }
    public int? ReceitaId { get; set; }
    public string? ReceitaNome { get; set; }
    public int? TempoPreparoMinutos { get; set; }
    public int? PorcoesDesejadas { get; set; }

    /// <summary>Por que a IA escolheu este prato (vazio quando a família trocou o prato).</summary>
    public string? Motivo { get; set; }
}
