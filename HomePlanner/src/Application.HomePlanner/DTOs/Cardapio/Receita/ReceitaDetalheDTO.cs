namespace Application.HomePlanner.DTOs.Cardapio.Receita;

public class ReceitaDetalheDTO
{
    public int Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string? ModoPreparo { get; init; }
    public int NumeroPorcoesBase { get; init; }
    public int? TempoPreparoMinutos { get; init; }
    public string? UrlOrigem { get; init; }
    public string? UrlImagem { get; init; }
    public string? Observacoes { get; init; }
    public DateTime DataCriacao { get; init; }
    public IReadOnlyList<ReceitaIngredienteDTO> Ingredientes { get; init; } = [];
}
