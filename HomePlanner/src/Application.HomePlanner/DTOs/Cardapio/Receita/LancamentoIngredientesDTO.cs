namespace Application.HomePlanner.DTOs.Cardapio.Receita;

/// <summary>
/// Resultado do diálogo "Lançar do texto": os ingredientes a aplicar e, se o usuário
/// pediu tradução, o idioma-alvo (para o formulário também traduzir o texto da receita).
/// </summary>
public class LancamentoIngredientesDTO
{
    public IReadOnlyList<IngredienteParseadoDTO> Ingredientes { get; init; } = [];

    /// <summary>"pt"/"en"/"es" quando o usuário marcou traduzir; null caso contrário.</summary>
    public string? IdiomaTraducao { get; init; }
}
