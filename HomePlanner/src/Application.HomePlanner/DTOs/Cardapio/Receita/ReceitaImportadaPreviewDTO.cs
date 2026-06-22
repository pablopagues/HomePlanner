namespace Application.HomePlanner.DTOs.Cardapio.Receita;

public class ReceitaImportadaPreviewDTO
{
    public bool Sucesso { get; init; }
    public string? MensagemErro { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string? ModoPreparo { get; init; }
    public int NumeroPorcoesBase { get; init; } = 4;
    public int? TempoPreparoMinutos { get; init; }
    public string? UrlOrigem { get; init; }
    public string? UrlImagem { get; init; }
    public IReadOnlyList<string> IngredientesTexto { get; init; } = [];
    public IReadOnlyList<IngredienteImportadoDTO> IngredientesParseados { get; set; } = [];

    public static ReceitaImportadaPreviewDTO Erro(string mensagem) =>
        new() { Sucesso = false, MensagemErro = mensagem };
}

public class IngredienteImportadoDTO
{
    public decimal? Quantidade { get; init; }
    public string? CodigoUnidade { get; init; }
    public string NomeIngrediente { get; init; } = string.Empty;
    /// <summary>Termo de preparo/corte separado do nome ("picada", "em cubos"), se houver.</summary>
    public string? Preparo { get; init; }
    public string TextoOriginal { get; init; } = string.Empty;
    public bool Opcional { get; init; }
}

/// <summary>
/// Linha de ingrediente já resolvida (vinculada a um ingrediente e unidade do cadastro),
/// pronta para virar uma linha de <c>ReceitaIngredientePersistenciaDTO</c>.
/// </summary>
public class IngredienteParseadoDTO
{
    public int IngredienteId { get; set; }
    public string NomeIngrediente { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public int UnidadeMedidaId { get; set; }
    public bool Opcional { get; set; }
    public string? Observacao { get; set; }
}
