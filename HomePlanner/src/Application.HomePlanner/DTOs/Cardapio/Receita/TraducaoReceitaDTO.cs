namespace Application.HomePlanner.DTOs.Cardapio.Receita;

/// <summary>Texto livre da receita traduzido para o idioma-alvo.</summary>
public class TraducaoReceitaDTO
{
    public string? Nome { get; init; }
    public string? ModoPreparo { get; init; }
    public string? Observacoes { get; init; }
}
