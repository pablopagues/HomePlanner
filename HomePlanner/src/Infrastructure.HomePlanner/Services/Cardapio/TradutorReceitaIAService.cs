using System.Text.Json;
using Anthropic;
using Anthropic.Models.Messages;
using Application.HomePlanner.DTOs.Cardapio.Receita;
using Application.HomePlanner.Services.Cardapio;
using Domain.HomePlanner.Models.SaaS.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.HomePlanner.Services.Cardapio;

/// <summary>
/// Traduz o texto livre da receita (nome, modo de preparo, observações) via Claude,
/// usando structured outputs. Retorna null em falha/refusal/desabilitado.
/// </summary>
public class TradutorReceitaIAService : ITradutorReceitaIA
{
    private readonly AnthropicOptions _options;
    private readonly ILogger<TradutorReceitaIAService> _logger;
    private readonly AnthropicClient? _client;

    public TradutorReceitaIAService(
        IOptions<AnthropicOptions> options,
        ILogger<TradutorReceitaIAService> logger)
    {
        _options = options.Value;
        _logger = logger;
        if (_options.IsEnabled)
            _client = new AnthropicClient { ApiKey = _options.ApiKey };
    }

    public bool Habilitado => _options.IsEnabled;

    public async Task<TraducaoReceitaDTO?> TraduzirAsync(
        string? nome, string? modoPreparo, string? observacoes,
        string idiomaAlvo, CancellationToken ct = default)
    {
        if (_client is null) return null;

        // Nada a traduzir.
        if (string.IsNullOrWhiteSpace(nome)
            && string.IsNullOrWhiteSpace(modoPreparo)
            && string.IsNullOrWhiteSpace(observacoes))
            return new TraducaoReceitaDTO { Nome = nome, ModoPreparo = modoPreparo, Observacoes = observacoes };

        try
        {
            var sistema =
                $"Você traduz textos de receitas culinárias para {ParserIngredientesIAService.NomeIdioma(idiomaAlvo)}. " +
                "Preserve a numeração e a estrutura dos passos do modo de preparo. Não invente conteúdo " +
                "nem adicione comentários. Para qualquer campo de entrada vazio, retorne null no JSON.";

            var usuario =
                $"NOME:\n{nome}\n\nMODO_DE_PREPARO:\n{modoPreparo}\n\nOBSERVACOES:\n{observacoes}";

            var parametros = new MessageCreateParams
            {
                Model = _options.Model,
                MaxTokens = 4096,
                System = sistema,
                Messages = [new() { Role = Role.User, Content = usuario }],
                OutputConfig = new OutputConfig { Format = new JsonOutputFormat { Schema = MontarSchema() } },
            };

            var resposta = await _client.Messages.Create(parametros, cancellationToken: ct);

            if (resposta.StopReason == "refusal")
            {
                _logger.LogWarning("Claude recusou a tradução da receita (refusal).");
                return null;
            }

            var texto = resposta.Content
                .Select(b => b.Value)
                .OfType<TextBlock>()
                .Select(t => t.Text)
                .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t));

            if (string.IsNullOrWhiteSpace(texto)) return null;

            using var doc = JsonDocument.Parse(texto!);
            var raiz = doc.RootElement;
            return new TraducaoReceitaDTO
            {
                Nome        = LerOuOriginal(raiz, "nome", nome),
                ModoPreparo = LerOuOriginal(raiz, "modoPreparo", modoPreparo),
                Observacoes = LerOuOriginal(raiz, "observacoes", observacoes),
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao traduzir a receita via IA.");
            return null;
        }
    }

    // Usa o valor traduzido; se vier vazio, mantém o original (não apaga campo).
    private static string? LerOuOriginal(JsonElement el, string prop, string? original)
    {
        if (el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String)
        {
            var s = v.GetString();
            if (!string.IsNullOrWhiteSpace(s)) return s;
        }
        return original;
    }

    private static Dictionary<string, JsonElement> MontarSchema()
    {
        const string schemaJson = """
        {
          "type": "object",
          "additionalProperties": false,
          "properties": {
            "nome": { "type": ["string", "null"] },
            "modoPreparo": { "type": ["string", "null"] },
            "observacoes": { "type": ["string", "null"] }
          },
          "required": ["nome", "modoPreparo", "observacoes"]
        }
        """;

        using var doc = JsonDocument.Parse(schemaJson);
        var schema = new Dictionary<string, JsonElement>();
        foreach (var prop in doc.RootElement.EnumerateObject())
            schema[prop.Name] = prop.Value.Clone();
        return schema;
    }
}
