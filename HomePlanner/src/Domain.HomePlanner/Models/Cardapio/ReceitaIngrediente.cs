namespace Domain.HomePlanner.Models.Cardapio;

public class ReceitaIngrediente
{
    public int Id { get; set; }
    public int ReceitaId { get; set; }
    public int IngredienteId { get; set; }
    public decimal Quantidade { get; set; }
    public int UnidadeMedidaId { get; set; }
    public string? Observacao { get; set; }
    public bool Opcional { get; set; }
    public int Ordem { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation
    public Receita Receita { get; set; } = null!;
    public Ingrediente Ingrediente { get; set; } = null!;
    public UnidadeMedida UnidadeMedida { get; set; } = null!;
}
