using Domain.HomePlanner.Models.Enums;

namespace Application.HomePlanner.DTOs.Planner;

public class TarefaListaDTO
{
    public int Id { get; init; }
    public string Titulo { get; init; } = string.Empty;
    public string? Descricao { get; init; }
    public DateOnly? DataPrevista { get; init; }
    public TimeOnly? HoraInicio { get; init; }
    public TimeOnly? HoraFim { get; init; }
    public bool Concluida { get; init; }
    public Recorrencia Recorrencia { get; init; }
    public VisibilidadeTarefa Visibilidade { get; init; }
    public string? ResponsavelUsuarioId { get; init; }
    public string? ResponsavelNome { get; init; }

    /// <summary>Data da última atualização da foto do responsável (null = sem foto). Usada para montar a URL/cache do avatar.</summary>
    public DateTime? ResponsavelFotoAtualizadaEm { get; init; }

    /// <summary>Token de versão da foto do responsável, ou null se ele não tem foto.</summary>
    public string? ResponsavelFotoVersao => ResponsavelFotoAtualizadaEm?.Ticks.ToString();
}
