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

    // ── Foto (uma por receita) ─────────────────────────────────────────────
    // Bytes já redimensionados/comprimidos de uma nova foto. Quando != null, o
    // save substitui a foto existente. Vazio = sem alteração.
    public byte[]? FotoConteudo { get; set; }
    public string? FotoContentType { get; set; }

    /// <summary>Pedido explícito de remoção da foto existente (ignora FotoConteudo).</summary>
    public bool RemoverFoto { get; set; }

    // Estado da foto já gravada (preenchido ao abrir a edição) — usado só pela UI
    // para exibir a prévia e os botões "alterar/remover".
    public bool TemFotoAtual { get; set; }
    public DateTime? FotoAtualizadaEm { get; set; }
}

public class ReceitaComponentePersistenciaDTO
{
    public int Id { get; set; }
    public int ReceitaComponenteId { get; set; }
    public string NomeComponente { get; set; } = string.Empty;
    public int PorcoesDesejadas { get; set; }
    public int Ordem { get; set; }
}
