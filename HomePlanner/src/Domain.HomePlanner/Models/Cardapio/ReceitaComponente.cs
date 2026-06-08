namespace Domain.HomePlanner.Models.Cardapio;

/// <summary>
/// Liga um prato composto (ReceitaPai) a outra receita usada como componente
/// (referência viva). A quantidade do componente é dada em porções.
/// </summary>
public class ReceitaComponente
{
    public int Id { get; set; }
    public int ReceitaPaiId { get; set; }
    public int ReceitaComponenteId { get; set; }
    public int PorcoesDesejadas { get; set; }
    public int Ordem { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation
    public Receita ReceitaPai { get; set; } = null!;
    public Receita Componente { get; set; } = null!;
}
