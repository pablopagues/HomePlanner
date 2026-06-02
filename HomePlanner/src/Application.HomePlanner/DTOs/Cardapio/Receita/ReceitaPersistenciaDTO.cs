namespace Application.HomePlanner.DTOs.Cardapio.Receita;

public class ReceitaPersistenciaDTO
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? ModoPreparo { get; set; }
    public int NumeroPorcoesBase { get; set; } = 4;
    public int? TempoPreparoMinutos { get; set; }
    public string? UrlOrigem { get; set; }
    public string? UrlImagem { get; set; }
    public string? Observacoes { get; set; }
    public List<ReceitaIngredientePersistenciaDTO> Ingredientes { get; set; } = [];
}
