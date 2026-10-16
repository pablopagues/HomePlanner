using Domain.HomePlanner.Models.Enums;
using Domain.HomePlanner.Models.SaaS.Interfaces;

namespace Domain.HomePlanner.Models.Cardapio;

/// <summary>
/// Rascunho automático do cardápio por IA: em que dia/hora (fuso da família) a IA
/// prepara a semana seguinte, e com quais regras. Uma linha por tenant.
/// </summary>
public class AgendamentoCardapioIA : ITenantEntity, IAuditable
{
    public Guid TenantId { get; set; }
    public bool Ativo { get; set; }

    /// <summary>Dia em que o rascunho da semana seguinte é preparado.</summary>
    public DiaSemana DiaSemana { get; set; } = DiaSemana.Sexta;

    /// <summary>Hora local (0–23) a partir da qual o rascunho é preparado.</summary>
    public int Hora { get; set; } = 9;

    /// <summary>Regras do cardápio (PedidoCardapioIADTO serializado, sem a data).</summary>
    public string ParametrosJson { get; set; } = "{}";

    // IAuditable
    public DateTime DataCriacao { get; set; }
    public DateTime? DataModificacao { get; set; }
    public string? CriadoPor { get; set; }
    public string? ModificadoPor { get; set; }
}
