namespace Application.HomePlanner.DTOs.Cardapio.Cardapio;

public class CardapioSemanaDTO
{
    public int Id { get; init; }
    public DateOnly DataInicio { get; init; }
    public string? Nome { get; init; }
    public IReadOnlyList<RefeicaoDiaDTO> Refeicoes { get; init; } = [];
}
