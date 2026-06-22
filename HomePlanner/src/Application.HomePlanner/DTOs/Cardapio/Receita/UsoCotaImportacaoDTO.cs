using Domain.HomePlanner.Models.Enums;

namespace Application.HomePlanner.DTOs.Cardapio.Receita;

/// <summary>Uso atual da cota de importação de receitas do tenant no mês corrente.</summary>
public class UsoCotaImportacaoDTO
{
    public PlanoAssinatura Plano { get; init; }
    public int Usado { get; init; }
    public int Limite { get; init; }
    public bool AtingiuLimite => Usado >= Limite;
}
