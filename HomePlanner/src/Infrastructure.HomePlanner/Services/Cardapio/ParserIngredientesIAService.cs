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
/// Parser de ingredientes via Claude (Anthropic), usando structured outputs para
/// devolver quantidade/unidade/nome/preparo já estruturados, em qualquer idioma.
/// Retorna null em caso de falha/refusal/desabilitado para o chamador cair no
/// parser regex de fallback.
/// </summary>
public class ParserIngredientesIAService : IParserIngredientesIA
{
    private readonly AnthropicOptions _options;
    private readonly ILogger<ParserIngredientesIAService> _logger;
    private readonly AnthropicClient? _client;

    // Códigos internos de unidade aceitos (devem casar com o UnidadeMedidaSeed).
    private static readonly string[] _codigosUnidade =
    {
        "g", "kg", "ml", "l", "xic", "cs", "cc", "pitada",
        "un", "dente", "fatia", "pacote",
        "oz", "lb", "floz", "cup", "pint", "quart",
    };

    public ParserIngredientesIAService(
        IOptions<AnthropicOptions> options,
        ILogger<ParserIngredientesIAService> logger)
    {
        _options = options.Value;
        _logger = logger;
        if (_options.IsEnabled)
            _client = new AnthropicClient { ApiKey = _options.ApiKey };
    }

    public bool Habilitado => _options.IsEnabled;

    public async Task<IReadOnlyList<IngredienteImportadoDTO>?> ParsearAsync(
        IReadOnlyList<string> linhas, CancellationToken ct = default)
    {
        if (_client is null) return null;

        var limpas = linhas
            .Select(l => l?.Trim() ?? string.Empty)
            .Where(l => l.Length > 0)
            .Take(_options.MaxIngredientesPorChamada)
            .ToList();

        if (limpas.Count == 0) return [];

        try
        {
            var parametros = new MessageCreateParams
            {
                Model = _options.Model,
                MaxTokens = 4096,
                System = SystemPrompt,
                Messages = [new() { Role = Role.User, Content = MontarUserPrompt(limpas) }],
                OutputConfig = new OutputConfig { Format = new JsonOutputFormat { Schema = MontarSchema() } },
            };

            var resposta = await _client.Messages.Create(parametros, cancellationToken: ct);

            if (resposta.StopReason == "refusal")
            {
                _logger.LogWarning("Claude recusou o parsing de ingredientes (refusal).");
                return null;
            }

            var texto = resposta.Content
                .Select(b => b.Value)
                .OfType<TextBlock>()
                .Select(t => t.Text)
                .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t));

            if (string.IsNullOrWhiteSpace(texto)) return null;

            return Mapear(texto!);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao parsear ingredientes via IA; usando fallback.");
            return null;
        }
    }

    private const string SystemPrompt =
        "Você é um extrator de ingredientes de receitas culinárias. Recebe uma lista de " +
        "linhas de ingredientes (uma por linha, em qualquer idioma) e devolve, no formato " +
        "JSON solicitado, uma lista estruturada na MESMA ORDEM e com a MESMA quantidade de " +
        "itens recebidos.\n" +
        "Regras por item:\n" +
        "- quantidade: número decimal (frações e faixas viram número; ex.: '1/2'→0.5, '1 a 2'→1). null se não houver.\n" +
        "- codigoUnidade: o código interno da unidade, ou null se não houver/for contável. " +
        "Escolha a unidade fiel ao texto original (receita em inglês com 'cup'→\"cup\", 'oz'→\"oz\"; em português 'xícara'→\"xic\", 'colher de sopa'→\"cs\").\n" +
        "- nome: o nome do produto comprável, limpo (sem quantidade/unidade). Para carnes/cortes, mantenha o corte como parte do nome.\n" +
        "- preparo: termo de preparo/estado separado do nome ('picada', 'em cubos', 'derretida'), ou null.\n" +
        "- opcional: true se a linha indicar 'opcional'/'a gosto'/'to taste' etc.\n" +
        "- textoOriginal: a linha original recebida, sem alterações.";

    private static string MontarUserPrompt(IReadOnlyList<string> linhas)
        => "Ingredientes:\n" + string.Join("\n", linhas);

    private Dictionary<string, JsonElement> MontarSchema()
    {
        var enumCodigos = string.Join(",", _codigosUnidade.Select(c => $"\"{c}\""));
        var schemaJson = $$"""
        {
          "type": "object",
          "additionalProperties": false,
          "properties": {
            "ingredientes": {
              "type": "array",
              "items": {
                "type": "object",
                "additionalProperties": false,
                "properties": {
                  "quantidade": { "type": ["number", "null"] },
                  "codigoUnidade": { "anyOf": [ { "type": "null" }, { "type": "string", "enum": [{{enumCodigos}}] } ] },
                  "nome": { "type": "string" },
                  "preparo": { "type": ["string", "null"] },
                  "opcional": { "type": "boolean" },
                  "textoOriginal": { "type": "string" }
                },
                "required": ["quantidade", "codigoUnidade", "nome", "preparo", "opcional", "textoOriginal"]
              }
            }
          },
          "required": ["ingredientes"]
        }
        """;

        using var doc = JsonDocument.Parse(schemaJson);
        var schema = new Dictionary<string, JsonElement>();
        foreach (var prop in doc.RootElement.EnumerateObject())
            schema[prop.Name] = prop.Value.Clone();
        return schema;
    }

    private static IReadOnlyList<IngredienteImportadoDTO> Mapear(string json)
    {
        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("ingredientes", out var arr)
            || arr.ValueKind != JsonValueKind.Array)
            return [];

        var lista = new List<IngredienteImportadoDTO>();
        foreach (var item in arr.EnumerateArray())
        {
            var nome = LerString(item, "nome");
            if (string.IsNullOrWhiteSpace(nome)) continue;

            lista.Add(new IngredienteImportadoDTO
            {
                Quantidade      = LerDecimal(item, "quantidade"),
                CodigoUnidade   = LerString(item, "codigoUnidade"),
                NomeIngrediente = nome.Trim(),
                Preparo         = LerString(item, "preparo"),
                TextoOriginal   = LerString(item, "textoOriginal") ?? nome,
                Opcional        = item.TryGetProperty("opcional", out var o)
                                  && o.ValueKind == JsonValueKind.True,
            });
        }
        return lista;
    }

    private static string? LerString(JsonElement el, string prop)
        => el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String
            ? v.GetString()
            : null;

    private static decimal? LerDecimal(JsonElement el, string prop)
        => el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.Number
            ? v.GetDecimal()
            : null;
}
