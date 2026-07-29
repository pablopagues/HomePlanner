using Application.HomePlanner.DTOs.Cardapio.Receita;
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
    public bool ReceitaTemFoto { get; init; }
    public DateTime? ReceitaFotoAtualizadaEm { get; init; }
    public int? PorcoesDesejadas { get; init; }
    public string? Observacao { get; init; }

    /// <summary>URL efetiva da imagem da receita (foto enviada tem precedência sobre UrlImagem).</summary>
    public string? ReceitaImagemSrc =>
        ReceitaImagemHelper.ResolverSrc(ReceitaId ?? 0, ReceitaTemFoto, ReceitaFotoAtualizadaEm, ReceitaUrlImagem);
}
