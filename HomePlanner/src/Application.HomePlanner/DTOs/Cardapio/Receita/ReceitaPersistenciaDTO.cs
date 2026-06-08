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
    public List<ReceitaComponentePersistenciaDTO> Componentes { get; set; } = [];
}

public class ReceitaComponentePersistenciaDTO
{
    public int Id { get; set; }
    public int ReceitaComponenteId { get; set; }
    public string NomeComponente { get; set; } = string.Empty;
    public int PorcoesDesejadas { get; set; }
    public int Ordem { get; set; }
}
