namespace Application.HomePlanner.DTOs.Cardapio.ModeloSemana;

public class ModeloSemanaListaDTO
{
    public int Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string? Descricao { get; init; }
    public int TotalRefeicoes { get; init; }
}
