using System.Globalization;
using System.Text;
using System.Text.Json;
using Anthropic;
using Anthropic.Models.Messages;
using Application.HomePlanner.DTOs.Cardapio.CardapioIA;
using Application.HomePlanner.Services.Cardapio;
using Domain.HomePlanner.Models.Enums;
using Domain.HomePlanner.Models.SaaS.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.HomePlanner.Services.Cardapio;

/// <summary>
/// Monta o cardápio da semana via Claude (structured outputs), escolhendo entre as
/// receitas da casa já filtradas pelas regras rígidas. Retorna null em falha/refusal.
/// </summary>
public class GeradorCardapioIAService : IGeradorCardapioIA
{
    private readonly AnthropicOptions _options;
    private readonly ILogger<GeradorCardapioIAService> _logger;
    private readonly AnthropicClient? _client;

    public GeradorCardapioIAService(
        IOptions<AnthropicOptions> options,
        ILogger<GeradorCardapioIAService> logger)
    {
        _options = options.Value;
        _logger = logger;
        if (_options.IsEnabled)
            _client = new AnthropicClient { ApiKey = _options.ApiKey };
    }

    public bool Habilitado => _options.IsEnabled;

    public async Task<RespostaCardapioIA?> GerarAsync(
        ContextoCardapioIA contexto, string idioma, CancellationToken ct = default)
    {
        if (_client is null || contexto.Horarios.Count == 0 || contexto.Candidatas.Count == 0)
            return null;

        var nomeIdioma = ParserIngredientesIAService.NomeIdioma(idioma);
        var regraRepetir = contexto.PermitirRepetirNaSemana
            ? "- Pode repetir uma receita na semana quando fizer sentido (ex.: sobra do jantar no almoço seguinte), mas prefira variedade.\n"
            : "- Não repita a mesma receita na semana, nem use as que já estão em <ja_na_semana>.\n";

        var sistema =
            "Você monta o cardápio da semana de uma família escolhendo pratos de uma lista de receitas " +
            "que a família já tem cadastradas.\n" +
            "Regras obrigatórias:\n" +
            "- Use somente receitaId da lista <receitas>. Nunca invente receitas nem ids.\n" +
            "- Preencha cada horário de <horarios> no máximo uma vez, com dia e tipo exatamente como listados.\n" +
            regraRepetir +
            "- Respeite o tempo máximo de preparo indicado no horário, quando a receita informa o tempo.\n" +
            "- Se nenhuma receita servir para um horário, deixe-o de fora em vez de forçar uma escolha ruim.\n" +
            "Critérios, em ordem de importância:\n" +
            "1. O pedido da família. Restrições alimentares citadas são obrigatórias.\n" +
            "2. Adequação ao tipo de refeição: use o histórico de uso de cada receita (ex.: 'Jantar 5x') e o " +
            "nome/ingredientes para não colocar, por exemplo, um prato de almoço no café da manhã.\n" +
            "3. Variedade: alterne proteínas e estilos ao longo da semana e evite o que a família comeu " +
            "recentemente (<recentes>).\n" +
            "4. Praticidade: sem outra indicação, prefira pratos mais rápidos em dias úteis.\n" +
            $"motivo: uma frase curta (até 12 palavras) em {nomeIdioma} explicando a escolha.\n" +
            $"observacoes: uma ou duas frases em {nomeIdioma} sobre o cardápio como um todo, ou null.";

        var json = await ChamarAsync(sistema, MontarMensagem(contexto), MontarSchema(contexto), ct);
        if (json is null) return null;

        try
        {
            using var doc = JsonDocument.Parse(json);
            var raiz = doc.RootElement;
            if (!raiz.TryGetProperty("refeicoes", out var arr) || arr.ValueKind != JsonValueKind.Array)
                return null;

            var escolhas = new List<EscolhaCardapioIA>();
            foreach (var el in arr.EnumerateArray())
            {
                if (!el.TryGetProperty("dia", out var d) || !d.TryGetInt32(out var dia)) continue;
                if (!el.TryGetProperty("receitaId", out var r) || !r.TryGetInt32(out var receitaId)) continue;
                var tipoTxt = el.TryGetProperty("tipo", out var t) ? t.GetString() : null;
                if (!Enum.TryParse<TipoRefeicao>(tipoTxt, out var tipo)) continue;
                if (!Enum.IsDefined(typeof(DiaSemana), dia)) continue;

                var motivo = el.TryGetProperty("motivo", out var m) && m.ValueKind == JsonValueKind.String
                    ? m.GetString()?.Trim()
                    : null;
                escolhas.Add(new EscolhaCardapioIA((DiaSemana)dia, tipo, receitaId, motivo));
            }

            var obs = raiz.TryGetProperty("observacoes", out var o) && o.ValueKind == JsonValueKind.String
                ? o.GetString()
                : null;
            return new RespostaCardapioIA { Escolhas = escolhas, Observacoes = obs };
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Resposta inválida da IA no cardápio da semana.");
            return null;
        }
    }

    // ─── Prompt ───────────────────────────────────────────────────────────────

    private static string MontarMensagem(ContextoCardapioIA c)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Porções por refeição: {c.Porcoes}.");
        sb.AppendLine("Pedido da família:");
        sb.AppendLine(c.Pedido ?? "(sem pedido específico — um cardápio variado e equilibrado para o dia a dia)");

        sb.AppendLine();
        sb.AppendLine("<horarios>");
        foreach (var h in c.Horarios)
        {
            sb.Append($"dia={(int)h.Dia} ({NomeDia(h.Dia)}, {Data(h.Data)}) tipo={h.Tipo}");
            if (h.TempoMaxMinutos is int max) sb.Append($" tempoMaximo={max}min");
            sb.AppendLine();
        }
        sb.AppendLine("</horarios>");

        if (c.JaNaSemana.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("<ja_na_semana>");
            foreach (var r in c.JaNaSemana)
                sb.AppendLine($"{NomeDia(r.Data.DayOfWeek)} {Data(r.Data)} {r.Tipo}: id={r.ReceitaId} {r.ReceitaNome}");
            sb.AppendLine("</ja_na_semana>");
        }

        if (c.Recentes.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("<recentes>");
            foreach (var r in c.Recentes)
                sb.AppendLine($"{Data(r.Data)} {r.Tipo}: {r.ReceitaNome}");
            sb.AppendLine("</recentes>");
        }

        sb.AppendLine();
        sb.AppendLine("<receitas>");
        foreach (var r in c.Candidatas)
        {
            sb.Append($"id={r.Id} | {r.Nome}");
            if (r.TempoPreparoMinutos is int t) sb.Append($" | {t} min");
            if (r.Ingredientes.Count > 0) sb.Append(" | ingredientes: ").Append(string.Join(", ", r.Ingredientes));
            if (r.UsosPorTipo.Count > 0)
                sb.Append(" | usos: ").Append(string.Join(", ",
                    r.UsosPorTipo.OrderByDescending(u => u.Value).Select(u => $"{u.Key} {u.Value}x")));
            if (r.UltimoUso is DateOnly u) sb.Append($" | último uso {Data(u)}");
            sb.AppendLine();
        }
        sb.AppendLine("</receitas>");
        return sb.ToString();
    }

    // dia e tipo restritos aos horários pedidos; o id é conferido em código (enum grande demais).
    private static Dictionary<string, JsonElement> MontarSchema(ContextoCardapioIA c)
    {
        var dias = string.Join(",", c.Horarios.Select(h => (int)h.Dia).Distinct().Order());
        var tipos = string.Join(",", c.Horarios.Select(h => $"\"{h.Tipo}\"").Distinct());
        var schemaJson = $$"""
        {
          "type": "object",
          "additionalProperties": false,
          "properties": {
            "refeicoes": {
              "type": "array",
              "items": {
                "type": "object",
                "additionalProperties": false,
                "properties": {
                  "dia": { "type": "integer", "enum": [{{dias}}] },
                  "tipo": { "type": "string", "enum": [{{tipos}}] },
                  "receitaId": { "type": "integer" },
                  "motivo": { "type": "string" }
                },
                "required": ["dia", "tipo", "receitaId", "motivo"]
              }
            },
            "observacoes": { "type": ["string", "null"] }
          },
          "required": ["refeicoes", "observacoes"]
        }
        """;

        using var doc = JsonDocument.Parse(schemaJson);
        var schema = new Dictionary<string, JsonElement>();
        foreach (var prop in doc.RootElement.EnumerateObject())
            schema[prop.Name] = prop.Value.Clone();
        return schema;
    }

    private static string NomeDia(DiaSemana d) => NomeDia(d switch
    {
        DiaSemana.Domingo => DayOfWeek.Sunday,
        _                 => (DayOfWeek)(int)d,
    });

    private static string NomeDia(DayOfWeek d) => d switch
    {
        DayOfWeek.Monday    => "segunda",
        DayOfWeek.Tuesday   => "terça",
        DayOfWeek.Wednesday => "quarta",
        DayOfWeek.Thursday  => "quinta",
        DayOfWeek.Friday    => "sexta",
        DayOfWeek.Saturday  => "sábado",
        _                   => "domingo",
    };

    private static string Data(DateOnly d) => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    // ─── Chamada ──────────────────────────────────────────────────────────────

    // Faz a chamada e devolve o JSON (texto) da resposta, ou null em falha/refusal.
    private async Task<string?> ChamarAsync(
        string sistema, string usuario, Dictionary<string, JsonElement> schema, CancellationToken ct)
    {
        try
        {
            var parametros = new MessageCreateParams
            {
                Model = _options.ModelCardapio,
                // Folga para o raciocínio (thinking adaptativo) além do JSON de ~21 horários.
                MaxTokens = 16000,
                System = sistema,
                Messages = [new() { Role = Role.User, Content = usuario }],
                OutputConfig = new OutputConfig
                {
                    Format = new JsonOutputFormat { Schema = schema },
                    // Médio: equilibra o cuidado com as restrições e o custo por geração.
                    Effort = Effort.Medium,
                },
            };

            var resposta = await _client!.Messages.Create(parametros, cancellationToken: ct);

            _logger.LogInformation(
                "Cardápio IA ({Modelo}): {Entrada} tokens de entrada, {Saida} de saída.",
                _options.ModelCardapio, resposta.Usage.InputTokens, resposta.Usage.OutputTokens);

            if (resposta.StopReason == "refusal")
            {
                _logger.LogWarning("Claude recusou o cardápio da semana (refusal).");
                return null;
            }
            if (resposta.StopReason == "max_tokens")
            {
                _logger.LogWarning("Resposta da IA truncada (max_tokens) no cardápio da semana.");
                return null;
            }

            return resposta.Content
                .Select(b => b.Value)
                .OfType<TextBlock>()
                .Select(t => t.Text)
                .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Falha no cardápio da semana via IA.");
            return null;
        }
    }
}
