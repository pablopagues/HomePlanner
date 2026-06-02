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
    public IReadOnlyList<IngredienteImportadoDTO> IngredientesParseados { get; init; } = [];

    public static ReceitaImportadaPreviewDTO Erro(string mensagem) =>
        new() { Sucesso = false, MensagemErro = mensagem };
}

public class IngredienteImportadoDTO
{
    public decimal? Quantidade { get; init; }
    public string? CodigoUnidade { get; init; }
    public string NomeIngrediente { get; init; } = string.Empty;
    public string TextoOriginal { get; init; } = string.Empty;
}
