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
    public bool TemFoto { get; init; }
    public DateTime? FotoAtualizadaEm { get; init; }
    public string? Observacoes { get; init; }
    public DateTime DataCriacao { get; init; }

    /// <summary>URL efetiva da imagem (foto enviada tem precedência sobre UrlImagem).</summary>
    public string? ImagemSrc => ReceitaImagemHelper.ResolverSrc(Id, TemFoto, FotoAtualizadaEm, UrlImagem);
    public IReadOnlyList<ReceitaIngredienteDTO> Ingredientes { get; init; } = [];

    /// <summary>Componentes (outras receitas) que compõem este prato, se houver.</summary>
    public IReadOnlyList<ReceitaComponenteDetalheDTO> Componentes { get; init; } = [];

    /// <summary>Ingredientes do prato inteiro, já expandidos (próprios + componentes). Preenchido pelo serviço.</summary>
    public IReadOnlyList<IngredienteExpandidoDTO> IngredientesExpandidos { get; set; } = [];

    public bool EhComposto => Componentes.Count > 0;
}

public class ReceitaComponenteDetalheDTO
{
    /// <summary>Id da linha ReceitaComponente (para reaproveitar na edição).</summary>
    public int Id { get; init; }
    public int ComponenteId { get; init; }
    public string Nome { get; init; } = string.Empty;
    public int PorcoesDesejadas { get; init; }
    public string? ModoPreparo { get; init; }
    public int Ordem { get; init; }
}
