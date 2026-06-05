using Domain.HomePlanner.Models.Enums;
using Domain.HomePlanner.Models.SaaS.Identity;
using Domain.HomePlanner.Models.SaaS.Interfaces;

namespace Domain.HomePlanner.Models.Planner;

public class Tarefa : ITenantEntity, IDeletableEntity, IAuditable
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }

    public DateOnly? DataPrevista { get; set; }

    /// <summary>Hora de início do agendamento (opcional). Só faz sentido com DataPrevista.</summary>
    public TimeOnly? HoraInicio { get; set; }
    /// <summary>Hora de término do agendamento (opcional). Deve ser >= HoraInicio.</summary>
    public TimeOnly? HoraFim { get; set; }

    public bool Concluida { get; set; }
    public DateTime? DataConclusao { get; set; }

    public Recorrencia Recorrencia { get; set; } = Recorrencia.Nenhuma;
    public VisibilidadeTarefa Visibilidade { get; set; } = VisibilidadeTarefa.Familia;

    /// <summary>Membro da família responsável (FK para Usuario). Null = sem responsável.</summary>
    public string? ResponsavelUsuarioId { get; set; }

    /// <summary>
    /// Usuário que criou a tarefa (Id do Usuario). Base da regra de visibilidade:
    /// tarefas <see cref="VisibilidadeTarefa.Privada"/> só aparecem para o criador.
    /// Diferente de <see cref="CriadoPor"/>, que é campo de auditoria e pode valer "system".
    /// </summary>
    public string? CriadoPorUsuarioId { get; set; }

    // IDeletableEntity
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedByUsuarioId { get; set; }

    // IAuditable
    public DateTime DataCriacao { get; set; }
    public DateTime? DataModificacao { get; set; }
    public string? CriadoPor { get; set; }
    public string? ModificadoPor { get; set; }

    // Navigation
    public Usuario? Responsavel { get; set; }
}
