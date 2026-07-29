namespace Application.HomePlanner.DTOs.Cardapio.Receita;

public class ReceitaListaDTO
{
    public int Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public int NumeroPorcoesBase { get; init; }
    public int? TempoPreparoMinutos { get; init; }
    public string? UrlImagem { get; init; }
    public bool TemFoto { get; init; }
    public DateTime? FotoAtualizadaEm { get; init; }
    public int TotalIngredientes { get; init; }
    public int TotalComponentes { get; init; }
    public DateTime DataCriacao { get; init; }

    /// <summary>URL efetiva da imagem (foto enviada tem precedência sobre UrlImagem).</summary>
    public string? ImagemSrc => ReceitaImagemHelper.ResolverSrc(Id, TemFoto, FotoAtualizadaEm, UrlImagem);
}
