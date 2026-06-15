using Domain.HomePlanner.Models.SaaS.Interfaces;

namespace Domain.HomePlanner.Models.Cardapio;

public class ConfiguracaoFamilia : ITenantEntity, IAuditable
{
    public Guid TenantId { get; set; }
    public string TiposRefeicaoAtivos { get; set; } = "Almoco";
    public int TamanhoFamiliaPadrao { get; set; } = 4;
    public string FusoHorario { get; set; } = "America/Toronto";

    /// <summary>
    /// Antecedência, em minutos, com que os lembretes de horário são disparados antes da hora da tarefa.
    /// Vale para todas as notificações com hora. Editável só pelo Owner. 0 = na hora exata.
    /// </summary>
    public int MinutosAntecedenciaLembrete { get; set; } = 15;

    // IAuditable
    public DateTime DataCriacao { get; set; }
    public DateTime? DataModificacao { get; set; }
    public string? CriadoPor { get; set; }
    public string? ModificadoPor { get; set; }
}
