using Domain.HomePlanner.Models.Enums;

namespace Application.HomePlanner.DTOs.Planner;

public class TarefaPersistenciaDTO
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public DateOnly? DataPrevista { get; set; }
    public TimeOnly? HoraInicio { get; set; }
    public TimeOnly? HoraFim { get; set; }
    public Recorrencia Recorrencia { get; set; } = Recorrencia.Nenhuma;
    public VisibilidadeTarefa Visibilidade { get; set; } = VisibilidadeTarefa.Familia;
    public string? ResponsavelUsuarioId { get; set; }
    public bool NotificarResponsaveis { get; set; }
}
