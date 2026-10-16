using Domain.HomePlanner.Models.Enums;
using Domain.HomePlanner.Models.SaaS.Interfaces;

namespace Domain.HomePlanner.Models.Cardapio;

/// <summary>
/// Cardápio que a IA preparou sozinha para uma semana, aguardando a família revisar.
/// Uma linha por tenant + semana: também é a trava que impede gerar duas vezes.
/// </summary>
public class RascunhoCardapioIA : ITenantEntity
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }

    /// <summary>Segunda-feira da semana do rascunho.</summary>
    public DateOnly DataInicio { get; set; }

    public StatusRascunhoCardapio Status { get; set; }

    /// <summary>CardapioSugeridoDTO serializado (quando Pronto/Aplicado/Descartado).</summary>
    public string? ConteudoJson { get; set; }

    /// <summary>Quantas vezes a geração foi tentada (erros temporários da IA).</summary>
    public int Tentativas { get; set; }

    /// <summary>Código do último erro (ErrosApp), para diagnóstico.</summary>
    public string? UltimoErro { get; set; }

    public DateTime CriadoEm { get; set; }
    public DateTime AtualizadoEm { get; set; }
}
