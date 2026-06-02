namespace Application.HomePlanner.DTOs.Cardapio.Receita;

public class ReceitaIngredienteDTO
{
    public int Id { get; init; }
    public int IngredienteId { get; init; }
    public string NomeIngrediente { get; init; } = string.Empty;
    public decimal Quantidade { get; init; }
    public int UnidadeMedidaId { get; init; }
    public string CodigoUnidade { get; init; } = string.Empty;
    public string NomeUnidade { get; init; } = string.Empty;
    public string? Observacao { get; init; }
    public bool Opcional { get; init; }
    public int Ordem { get; init; }
}

public class ReceitaIngredientePersistenciaDTO
{
    public int Id { get; set; }
    public int IngredienteId { get; set; }
    public decimal Quantidade { get; set; }
    public int UnidadeMedidaId { get; set; }
    public string? Observacao { get; set; }
    public bool Opcional { get; set; }
    public int Ordem { get; set; }
}
