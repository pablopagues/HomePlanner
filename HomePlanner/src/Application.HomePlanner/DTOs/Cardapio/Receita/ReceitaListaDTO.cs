namespace Application.HomePlanner.DTOs.Cardapio.Receita;

public class ReceitaListaDTO
{
    public int Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public int NumeroPorcoesBase { get; init; }
    public int? TempoPreparoMinutos { get; init; }
    public string? UrlImagem { get; init; }
    public int TotalIngredientes { get; init; }
    public int TotalComponentes { get; init; }
    public DateTime DataCriacao { get; init; }
}
