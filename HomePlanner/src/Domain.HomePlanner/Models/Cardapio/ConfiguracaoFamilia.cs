using Domain.HomePlanner.Models.SaaS.Interfaces;

namespace Domain.HomePlanner.Models.Cardapio;

public class ConfiguracaoFamilia : ITenantEntity, IAuditable
{
    public Guid TenantId { get; set; }
    public string TiposRefeicaoAtivos { get; set; } = "Almoco";
    public int TamanhoFamiliaPadrao { get; set; } = 4;
    public string FusoHorario { get; set; } = "America/Toronto";

    // IAuditable
    public DateTime DataCriacao { get; set; }
    public DateTime? DataModificacao { get; set; }
    public string? CriadoPor { get; set; }
    public string? ModificadoPor { get; set; }
}
