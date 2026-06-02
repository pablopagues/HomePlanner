namespace Application.HomePlanner.DTOs.Cardapio.ModeloSemana;

public class ModeloSemanaDetalheDTO
{
    public int Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string? Descricao { get; init; }
    public IReadOnlyList<ModeloSemanaRefeicaoDTO> Refeicoes { get; init; } = [];
}
