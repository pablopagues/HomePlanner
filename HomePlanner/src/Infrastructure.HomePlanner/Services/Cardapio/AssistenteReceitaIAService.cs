using System.Text;
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
/// Extração de receita a partir de texto livre e sugestão de receitas via Claude,
/// usando structured outputs. Retorna null em falha/refusal/desabilitado.
/// </summary>
public class AssistenteReceitaIAService : IAssistenteReceitaIA
{
    private const int QuantidadeSugestoes = 3;

    private readonly AnthropicOptions _options;
    private readonly ILogger<AssistenteReceitaIAService> _logger;
    private readonly AnthropicClient? _client;

    public AssistenteReceitaIAService(
        IOptions<AnthropicOptions> options,
        ILogger<AssistenteReceitaIAService> logger)
    {
        _options = options.Value;
        _logger = logger;
        if (_options.IsEnabled)
            _client = new AnthropicClient { ApiKey = _options.ApiKey };
    }

    public bool Habilitado => _options.IsEnabled;

    public int MaxCaracteresTexto => _options.MaxCaracteresTextoReceita;

    // ─── Extração de texto colado ─────────────────────────────────────────────

    private const string SystemExtracao =
        "Você extrai receitas culinárias de textos livres copiados de sites, de conversas com " +
        "assistentes de IA (como o ChatGPT), de mensagens etc. O texto pode conter conversa, " +
        "comentários, emojis e marcação markdown — ignore o que não é a receita.\n" +
        "Regras:\n" +
        "- Não traduza: mantenha o idioma do texto original.\n" +
        "- Não invente nada que não esteja no texto.\n" +
        "- nome: o nome do prato.\n" +
        "- porcoes: número de porções/pessoas (inteiro), ou null se não informado.\n" +
        "- tempoMinutos: tempo total de preparo em minutos (inteiro), ou null se não informado.\n" +
        "- ingredientes: um ingrediente por item, com quantidade, unidade e preparo como escritos " +
        "no texto (ex.: '2 xícaras de farinha de trigo', '1 cebola picada'). Remova marcadores, " +
        "numeração e markdown. Não inclua títulos de seção ('Para a massa:') como ingrediente.\n" +
        "- modoPreparo: os passos numerados ('1. ...'), separados por uma linha em branco, com o " +
        "texto fiel ao original. Se houver dicas ou observações do autor, inclua-as ao final " +
        "precedidas de 'Dicas:'. null se não houver modo de preparo.\n" +
        "- encontrouReceita: false se o texto não contiver uma receita (aí os demais campos podem ser vazios/null).";

    public async Task<ReceitaImportadaPreviewDTO?> ExtrairDeTextoAsync(string texto, CancellationToken ct = default)
    {
        if (_client is null || string.IsNullOrWhiteSpace(texto)) return null;

        var json = await ChamarAsync(
            _options.Model, 4096, SystemExtracao,
            "Texto:\n<texto>\n" + texto.Trim() + "\n</texto>",
            MontarSchema(SchemaReceitaComFlag), "extração de receita", ct);
        if (json is null) return null;

        try
        {
            using var doc = JsonDocument.Parse(json);
            var raiz = doc.RootElement;
            var encontrou = raiz.TryGetProperty("encontrouReceita", out var e) && e.ValueKind == JsonValueKind.True;
            var receita = MapearReceita(raiz);
            if (!encontrou || receita is null)
                return ReceitaImportadaPreviewDTO.Erro("Receita não identificada no texto.");
            return receita;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Resposta inválida da IA na extração de receita.");
            return null;
        }
    }

    // ─── Sugestões ────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<ReceitaImportadaPreviewDTO>?> SugerirAsync(
        PedidoSugestaoReceitaDTO pedido,
        IReadOnlyList<string> receitasDaCasa,
        string idioma,
        CancellationToken ct = default)
    {
        if (_client is null) return null;

        // Idioma: o do pedido escrito (é o idioma em que a família pensa a receita); sem pedido,
        // o da interface. Todos os campos no MESMO idioma — antes o modelo misturava nome em um
        // idioma e descrição em outro quando pedido/receitas da casa divergiam da interface.
        var regraIdioma = string.IsNullOrWhiteSpace(pedido.Texto)
            ? $"Escreva TODOS os campos (nome, descricao, ingredientes, modoPreparo) em {ParserIngredientesIAService.NomeIdioma(idioma)}."
            : "Escreva TODOS os campos (nome, descricao, ingredientes, modoPreparo) no idioma em que o " +
              "pedido da família foi escrito, mesmo que as receitas da família estejam em outro idioma. " +
              $"Se não for possível identificar o idioma do pedido, use {ParserIngredientesIAService.NomeIdioma(idioma)}.";

        var sistema =
            "Você é um chef de cozinha doméstica que sugere receitas para uma família.\n" +
            regraIdioma + " Nunca misture idiomas entre os campos ou entre as receitas.\n" +
            $"Gere exatamente {QuantidadeSugestoes} receitas bem diferentes entre si que atendam ao pedido.\n" +
            "Regras:\n" +
            "- Receitas reais e testadas, com ingredientes fáceis de achar em supermercado.\n" +
            "- descricao: uma ou duas frases dizendo por que a receita combina com o pedido.\n" +
            "- ingredientes: um por item, com quantidade e unidade (ex.: '2 xícaras de farinha de trigo', " +
            "'500 g de peito de frango em cubos'), usando as unidades usuais do idioma da resposta.\n" +
            "- modoPreparo: passos numerados ('1. ...'), separados por uma linha em branco.\n" +
            "- porcoes e tempoMinutos: inteiros realistas.\n" +
            "- Não sugira receitas que a família já tem nem as da lista 'já sugeridas'; use as " +
            "receitas da família só para entender o gosto da casa.\n" +
            "- Respeite rigorosamente restrições alimentares citadas no pedido.";

        var sb = new StringBuilder();
        sb.AppendLine("Pedido da família:");
        sb.AppendLine(string.IsNullOrWhiteSpace(pedido.Texto)
            ? "(sem pedido específico — surpreenda com receitas variadas para o dia a dia)"
            : pedido.Texto.Trim());
        if (pedido.TempoMaximoMinutos is int tempo && tempo > 0)
            sb.AppendLine($"Tempo máximo de preparo: {tempo} minutos.");
        if (pedido.Porcoes is int porcoes && porcoes > 0)
            sb.AppendLine($"Porções: {porcoes}.");
        if (receitasDaCasa.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("<receitas_da_familia>");
            foreach (var r in receitasDaCasa) sb.AppendLine(r);
            sb.AppendLine("</receitas_da_familia>");
        }
        if (pedido.JaSugeridas.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("<ja_sugeridas>");
            foreach (var r in pedido.JaSugeridas) sb.AppendLine(r);
            sb.AppendLine("</ja_sugeridas>");
        }

        var schema = $$"""
        {
          "type": "object",
          "additionalProperties": false,
          "properties": {
            "sugestoes": { "type": "array", "items": {{SchemaReceita}} }
          },
          "required": ["sugestoes"]
        }
        """;

        var json = await ChamarAsync(
            _options.ModelSugestao, 8192, sistema, sb.ToString(),
            MontarSchema(schema), "sugestão de receitas", ct);
        if (json is null) return null;

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("sugestoes", out var arr) || arr.ValueKind != JsonValueKind.Array)
                return null;

            var lista = arr.EnumerateArray()
                .Select(MapearReceita)
                .OfType<ReceitaImportadaPreviewDTO>()
                .Take(QuantidadeSugestoes)
                .ToList();
            return lista.Count > 0 ? lista : null;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Resposta inválida da IA na sugestão de receitas.");
            return null;
        }
    }

    // ─── Infra comum ──────────────────────────────────────────────────────────

    private const string SchemaReceita = """
    {
      "type": "object",
      "additionalProperties": false,
      "properties": {
        "nome": { "type": "string" },
        "descricao": { "type": ["string", "null"] },
        "porcoes": { "type": ["integer", "null"] },
        "tempoMinutos": { "type": ["integer", "null"] },
        "ingredientes": { "type": "array", "items": { "type": "string" } },
        "modoPreparo": { "type": ["string", "null"] }
      },
      "required": ["nome", "descricao", "porcoes", "tempoMinutos", "ingredientes", "modoPreparo"]
    }
    """;

    private const string SchemaReceitaComFlag = """
    {
      "type": "object",
      "additionalProperties": false,
      "properties": {
        "encontrouReceita": { "type": "boolean" },
        "nome": { "type": "string" },
        "porcoes": { "type": ["integer", "null"] },
        "tempoMinutos": { "type": ["integer", "null"] },
        "ingredientes": { "type": "array", "items": { "type": "string" } },
        "modoPreparo": { "type": ["string", "null"] }
      },
      "required": ["encontrouReceita", "nome", "porcoes", "tempoMinutos", "ingredientes", "modoPreparo"]
    }
    """;

    // Faz a chamada e devolve o JSON (texto) da resposta, ou null em falha/refusal.
    private async Task<string?> ChamarAsync(
        string modelo, int maxTokens, string sistema, string usuario,
        Dictionary<string, JsonElement> schema, string operacao, CancellationToken ct)
    {
        try
        {
            var parametros = new MessageCreateParams
            {
                Model = modelo,
                MaxTokens = maxTokens,
                System = sistema,
                Messages = [new() { Role = Role.User, Content = usuario }],
                OutputConfig = new OutputConfig { Format = new JsonOutputFormat { Schema = schema } },
            };

            var resposta = await _client!.Messages.Create(parametros, cancellationToken: ct);

            if (resposta.StopReason == "refusal")
            {
                _logger.LogWarning("Claude recusou a {Operacao} (refusal).", operacao);
                return null;
            }
            if (resposta.StopReason == "max_tokens")
            {
                _logger.LogWarning("Resposta da IA truncada (max_tokens) na {Operacao}.", operacao);
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
            _logger.LogWarning(ex, "Falha na {Operacao} via IA.", operacao);
            return null;
        }
    }

    private static ReceitaImportadaPreviewDTO? MapearReceita(JsonElement el)
    {
        var nome = LerString(el, "nome");
        var ingredientes = el.TryGetProperty("ingredientes", out var arr) && arr.ValueKind == JsonValueKind.Array
            ? arr.EnumerateArray()
                .Where(i => i.ValueKind == JsonValueKind.String)
                .Select(i => i.GetString()!.Trim())
                .Where(s => s.Length > 0)
                .ToList()
            : [];
        var modoPreparo = LerString(el, "modoPreparo");

        // Sem nome e sem conteúdo não é receita.
        if (string.IsNullOrWhiteSpace(nome) && ingredientes.Count == 0 && string.IsNullOrWhiteSpace(modoPreparo))
            return null;

        var porcoes = LerInt(el, "porcoes");
        var tempo = LerInt(el, "tempoMinutos");

        return new ReceitaImportadaPreviewDTO
        {
            Sucesso             = true,
            Nome                = nome?.Trim() ?? string.Empty,
            Descricao           = LerString(el, "descricao"),
            NumeroPorcoesBase   = porcoes is > 0 ? porcoes.Value : 4,
            TempoPreparoMinutos = tempo is > 0 ? tempo : null,
            ModoPreparo         = string.IsNullOrWhiteSpace(modoPreparo) ? null : modoPreparo.Trim(),
            IngredientesTexto   = ingredientes,
        };
    }

    private static Dictionary<string, JsonElement> MontarSchema(string schemaJson)
    {
        using var doc = JsonDocument.Parse(schemaJson);
        var schema = new Dictionary<string, JsonElement>();
        foreach (var prop in doc.RootElement.EnumerateObject())
            schema[prop.Name] = prop.Value.Clone();
        return schema;
    }

    private static string? LerString(JsonElement el, string prop)
        => el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String
            ? v.GetString()
            : null;

    private static int? LerInt(JsonElement el, string prop)
        => el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.Number && v.TryGetInt32(out var i)
            ? i
            : null;
}
