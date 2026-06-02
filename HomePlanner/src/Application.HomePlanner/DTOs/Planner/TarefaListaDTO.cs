using Domain.HomePlanner.Models.Enums;

namespace Application.HomePlanner.DTOs.Planner;

public class TarefaListaDTO
{
    public int Id { get; init; }
    public string Titulo { get; init; } = string.Empty;
    public string? Descricao { get; init; }
    public DateOnly? DataPrevista { get; init; }
    public bool Concluida { get; init; }
    public Recorrencia Recorrencia { get; init; }
    public VisibilidadeTarefa Visibilidade { get; init; }
    public string? ResponsavelUsuarioId { get; init; }
    public string? ResponsavelNome { get; init; }
}
