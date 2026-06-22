using Domain.HomePlanner.Models.SaaS.Interfaces;

namespace Domain.HomePlanner.Models.Cardapio;

/// <summary>
/// Contador mensal de importações de receita (via URL) por tenant, usado para
/// aplicar a cota do plano. Uma linha por tenant + mês-calendário; o
/// <see cref="Quantidade"/> só incrementa (robusto contra importar→deletar→importar).
/// </summary>
public class UsoImportacaoReceita : ITenantEntity
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }

    /// <summary>Mês-calendário no formato yyyyMM (ex.: 202606).</summary>
    public int AnoMes { get; set; }

    public int Quantidade { get; set; }
}
