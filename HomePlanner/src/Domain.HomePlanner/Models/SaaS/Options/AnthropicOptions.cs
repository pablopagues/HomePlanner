namespace Domain.HomePlanner.Models.SaaS.Options;

/// <summary>
/// Configuração da integração com a API da Anthropic (Claude), usada para o
/// parsing inteligente de ingredientes de receitas em qualquer idioma.
/// A <see cref="ApiKey"/> é secreta: configurar via User Secrets (dev) ou
/// variável de ambiente (servidor) — nunca commitar no appsettings.
/// </summary>
public class AnthropicOptions
{
    public const string SectionName = "Anthropic";

    /// <summary>Chave da API (sk-ant-...). Vazia = integração desligada.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Modelo a usar. Padrão: Haiku 4.5 (barato e suficiente para parsing).</summary>
    public string Model { get; set; } = "claude-haiku-4-5";

    /// <summary>Teto de itens parseados por chamada (proteção de custo/payload).</summary>
    public int MaxIngredientesPorChamada { get; set; } = 60;

    /// <summary>true quando há chave configurada — habilita o parser por IA.</summary>
    public bool IsEnabled => !string.IsNullOrWhiteSpace(ApiKey);
}
