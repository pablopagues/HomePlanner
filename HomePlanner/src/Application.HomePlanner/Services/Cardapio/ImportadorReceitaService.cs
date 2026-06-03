using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Cardapio.Receita;
using Application.HomePlanner.Repositories.Cardapio;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Application.HomePlanner.Services.Cardapio;

public class ImportadorReceitaService : IImportadorReceitaService
{
    private readonly HttpClient _http;
    private readonly IUnidadeMedidaRepository _unidadeRepo;
    private readonly ILogger<ImportadorReceitaService> _logger;

    // Mapeamento de unidades comuns em inglês/português → codigo interno
    private static readonly Dictionary<string, string> _mapaUnidades = new(StringComparer.OrdinalIgnoreCase)
    {
        ["g"] = "g", ["grama"] = "g", ["gramas"] = "g", ["gram"] = "g", ["grams"] = "g",
        ["kg"] = "kg", ["quilograma"] = "kg", ["kilogram"] = "kg", ["kilograms"] = "kg",
        ["ml"] = "ml", ["mililitro"] = "ml", ["mililitros"] = "ml", ["milliliter"] = "ml", ["milliliters"] = "ml",
        ["l"] = "l", ["litro"] = "l", ["litros"] = "l", ["liter"] = "l", ["liters"] = "l",
        ["xícara"] = "xic", ["xicara"] = "xic", ["cup"] = "xic", ["cups"] = "xic",
        ["colher de sopa"] = "cs", ["tablespoon"] = "cs", ["tablespoons"] = "cs", ["tbsp"] = "cs",
        ["colher de chá"] = "cc", ["teaspoon"] = "cc", ["teaspoons"] = "cc", ["tsp"] = "cc",
        ["pitada"] = "pitada", ["pinch"] = "pitada",
        ["unidade"] = "un", ["unidades"] = "un", ["unit"] = "un", ["units"] = "un",
        ["dente"] = "dente", ["dentes"] = "dente", ["clove"] = "dente", ["cloves"] = "dente",
        ["fatia"] = "fatia", ["fatias"] = "fatia", ["slice"] = "fatia", ["slices"] = "fatia",
        ["pacote"] = "pacote", ["pacotes"] = "pacote", ["package"] = "pacote",
    };

    public ImportadorReceitaService(
        HttpClient http,
        IUnidadeMedidaRepository unidadeRepo,
        ILogger<ImportadorReceitaService> logger)
    {
        _http = http;
        _unidadeRepo = unidadeRepo;
        _logger = logger;
    }

    public async Task<ResultadoOperacao<ReceitaImportadaPreviewDTO>> ImportarDeUrlAsync(
        string url, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            return ResultadoOperacao<ReceitaImportadaPreviewDTO>.Falha("URL não informada.");

        if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri)
            || (uri.Scheme != "http" && uri.Scheme != "https"))
            return ResultadoOperacao<ReceitaImportadaPreviewDTO>.Falha("URL inválida.");

        string html;
        try
        {
            _http.DefaultRequestHeaders.UserAgent.TryParseAdd(
                "Mozilla/5.0 (compatible; HomePlannerBot/1.0)");
            html = await _http.GetStringAsync(uri, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao buscar URL {Url}", url);
            return ResultadoOperacao<ReceitaImportadaPreviewDTO>.Falha(
                $"Não foi possível acessar a URL: {ex.Message}");
        }

        var preview = ExtrairDeJsonLd(html, url.Trim())
                   ?? ExtrairHeuristico(html, url.Trim());

        if (preview is null)
            return ResultadoOperacao<ReceitaImportadaPreviewDTO>.Falha(
                "Não foi possível identificar uma receita nesta página. " +
                "Verifique se o site usa o formato schema.org/Recipe.");

        return ResultadoOperacao<ReceitaImportadaPreviewDTO>.Ok(preview);
    }

    // ─── Parsing de texto livre ───────────────────────────────────────────────

    public IReadOnlyList<IngredienteImportadoDTO> ParsearTexto(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return [];

        var resultado = new List<IngredienteImportadoDTO>();
        foreach (var bruta in texto.Split('\n'))
        {
            var linha = bruta.Trim().TrimStart('-', '•', '*', '·', '–', '—', ' ').Trim();
            if (linha.Length == 0 || EhCabecalho(linha)) continue;
            resultado.Add(ParsearLinhaIngrediente(linha));
        }
        return resultado;
    }

    // Linhas tipo "Ingredientes originais:" ou "Ingredientes:" não são itens.
    private static bool EhCabecalho(string linha)
        => linha.EndsWith(':') && Regex.IsMatch(linha, "ingrediente", RegexOptions.IgnoreCase);

    // ─── JSON-LD (schema.org/Recipe) ──────────────────────────────────────────

    private ReceitaImportadaPreviewDTO? ExtrairDeJsonLd(string html, string urlOrigem)
    {
        // Extrai todos os blocos <script type="application/ld+json">
        var blocos = Regex.Matches(
            html,
            @"<script[^>]*type=[""']application/ld\+json[""'][^>]*>(.*?)</script>",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        foreach (Match bloco in blocos)
        {
            var json = bloco.Groups[1].Value.Trim();
            try
            {
                using var doc = JsonDocument.Parse(json);
                var raiz = doc.RootElement;

                // Suporte a @graph
                if (raiz.TryGetProperty("@graph", out var graph) && graph.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in graph.EnumerateArray())
                    {
                        var resultado = TentarMapearReceita(item, urlOrigem);
                        if (resultado is not null) return resultado;
                    }
                }

                // Array na raiz
                if (raiz.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in raiz.EnumerateArray())
                    {
                        var resultado = TentarMapearReceita(item, urlOrigem);
                        if (resultado is not null) return resultado;
                    }
                }

                // Objeto simples
                return TentarMapearReceita(raiz, urlOrigem);
            }
            catch (JsonException ex)
            {
                _logger.LogDebug(ex, "JSON-LD inválido ignorado.");
            }
        }
        return null;
    }

    private static ReceitaImportadaPreviewDTO? TentarMapearReceita(JsonElement el, string urlOrigem)
    {
        if (!EhTipoReceita(el)) return null;

        var nome = LerString(el, "name") ?? LerString(el, "headline") ?? string.Empty;
        if (string.IsNullOrWhiteSpace(nome)) return null;

        var ingredientesTexto = LerStringArray(el, "recipeIngredient");
        var modoPreparo = ExtrairInstrucoes(el);
        var porcoes = ParsearPorcoes(LerString(el, "recipeYield"));
        var tempo = ParsearDuracaoIso(LerString(el, "totalTime") ?? LerString(el, "cookTime") ?? LerString(el, "prepTime"));
        var imagem = LerImagem(el);

        var parseados = ingredientesTexto.Select(ParsearLinhaIngrediente).ToList();

        return new ReceitaImportadaPreviewDTO
        {
            Sucesso             = true,
            Nome                = nome.Trim(),
            ModoPreparo         = modoPreparo,
            NumeroPorcoesBase   = porcoes,
            TempoPreparoMinutos = tempo,
            UrlOrigem           = urlOrigem,
            UrlImagem           = imagem,
            IngredientesTexto   = ingredientesTexto,
            IngredientesParseados = parseados,
        };
    }

    private static bool EhTipoReceita(JsonElement el)
    {
        if (!el.TryGetProperty("@type", out var tipo)) return false;
        if (tipo.ValueKind == JsonValueKind.String)
            return tipo.GetString()?.Contains("Recipe", StringComparison.OrdinalIgnoreCase) ?? false;
        if (tipo.ValueKind == JsonValueKind.Array)
            return tipo.EnumerateArray().Any(t =>
                t.GetString()?.Contains("Recipe", StringComparison.OrdinalIgnoreCase) ?? false);
        return false;
    }

    private static string? ExtrairInstrucoes(JsonElement el)
    {
        if (!el.TryGetProperty("recipeInstructions", out var inst)) return null;

        if (inst.ValueKind == JsonValueKind.String)
            return LimparHtml(inst.GetString() ?? string.Empty);

        if (inst.ValueKind == JsonValueKind.Array)
        {
            var passos = inst.EnumerateArray()
                .Select(p =>
                {
                    if (p.ValueKind == JsonValueKind.String) return p.GetString() ?? "";
                    return LerString(p, "text") ?? LerString(p, "name") ?? "";
                })
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            return string.Join("\n\n", passos.Select((s, i) => $"{i + 1}. {s.Trim()}"));
        }
        return null;
    }

    private static IReadOnlyList<string> LerStringArray(JsonElement el, string prop)
    {
        if (!el.TryGetProperty(prop, out var v)) return [];
        if (v.ValueKind == JsonValueKind.String) return [v.GetString() ?? ""];
        if (v.ValueKind == JsonValueKind.Array)
            return v.EnumerateArray()
                .Where(e => e.ValueKind == JsonValueKind.String)
                .Select(e => e.GetString() ?? "")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
        return [];
    }

    private static string? LerString(JsonElement el, string prop)
    {
        if (!el.TryGetProperty(prop, out var v)) return null;
        return v.ValueKind == JsonValueKind.String ? v.GetString() : null;
    }

    private static string? LerImagem(JsonElement el)
    {
        if (!el.TryGetProperty("image", out var img)) return null;
        if (img.ValueKind == JsonValueKind.String) return img.GetString();
        if (img.ValueKind == JsonValueKind.Object) return LerString(img, "url");
        if (img.ValueKind == JsonValueKind.Array && img.GetArrayLength() > 0)
        {
            var primeiro = img[0];
            if (primeiro.ValueKind == JsonValueKind.String) return primeiro.GetString();
            if (primeiro.ValueKind == JsonValueKind.Object) return LerString(primeiro, "url");
        }
        return null;
    }

    // ─── Heurístico (fallback) ────────────────────────────────────────────────

    private static ReceitaImportadaPreviewDTO? ExtrairHeuristico(string html, string urlOrigem)
    {
        // Tenta extrair título via og:title ou title
        var titulo = Regex.Match(html, @"og:title[^>]*content=[""']([^""']+)[""']", RegexOptions.IgnoreCase).Groups[1].Value.Trim();
        if (string.IsNullOrWhiteSpace(titulo))
            titulo = Regex.Match(html, @"<title[^>]*>([^<]+)</title>", RegexOptions.IgnoreCase).Groups[1].Value.Trim();
        if (string.IsNullOrWhiteSpace(titulo)) return null;

        var imagem = Regex.Match(html, @"og:image[^>]*content=[""']([^""']+)[""']", RegexOptions.IgnoreCase).Groups[1].Value.Trim();

        return new ReceitaImportadaPreviewDTO
        {
            Sucesso           = true,
            Nome              = titulo,
            UrlOrigem         = urlOrigem,
            UrlImagem         = string.IsNullOrWhiteSpace(imagem) ? null : imagem,
            IngredientesTexto = [],
            IngredientesParseados = [],
        };
    }

    // ─── Parsers auxiliares ───────────────────────────────────────────────────

    private static int ParsearPorcoes(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return 4;
        var match = Regex.Match(texto, @"(\d+)");
        return match.Success ? int.Parse(match.Groups[1].Value) : 4;
    }

    private static int? ParsearDuracaoIso(string? iso)
    {
        if (string.IsNullOrWhiteSpace(iso)) return null;
        // Ex: PT1H30M, PT45M
        var m = Regex.Match(iso, @"PT(?:(\d+)H)?(?:(\d+)M)?", RegexOptions.IgnoreCase);
        if (!m.Success) return null;
        int horas = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : 0;
        int minutos = m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 0;
        var total = horas * 60 + minutos;
        return total > 0 ? total : null;
    }

    private static IngredienteImportadoDTO ParsearLinhaIngrediente(string linha)
    {
        linha = LimparHtml(linha).Trim();

        // "(opcional)", "opcional" ou "a gosto" → marca como opcional.
        bool opcional = Regex.IsMatch(linha, @"\bopcional\b|\ba gosto\b", RegexOptions.IgnoreCase);

        // Padrão: "2 xícaras de farinha", "500g de frango", "1/2 cup sugar"
        var match = Regex.Match(linha,
            @"^([\d,./]+(?:\s+\d+/\d+)?)\s*([a-záàãâéêíóôõúüç]+(?:\s+de\s+[a-z]+)?)\s+(?:de\s+)?(.+)$",
            RegexOptions.IgnoreCase);

        if (!match.Success)
            return new IngredienteImportadoDTO
            {
                NomeIngrediente = LimparNome(linha),
                TextoOriginal   = linha,
                Opcional        = opcional,
            };

        var quantidadeStr = match.Groups[1].Value;
        var unidadeStr    = match.Groups[2].Value.Trim().ToLowerInvariant();
        var resto         = match.Groups[3].Value.Trim();

        var quantidade = ParsearQuantidade(quantidadeStr);

        string? codigoUnidade = null;
        string nomeIngrediente;
        if (_mapaUnidades.TryGetValue(unidadeStr, out var codigo))
        {
            // A palavra capturada é uma unidade conhecida.
            codigoUnidade   = codigo;
            nomeIngrediente = resto;
        }
        else
        {
            // Não é unidade (ex.: "3 ovos batidos") → a palavra faz parte do nome.
            nomeIngrediente = $"{match.Groups[2].Value} {resto}".Trim();
        }

        return new IngredienteImportadoDTO
        {
            Quantidade      = quantidade,
            CodigoUnidade   = codigoUnidade,
            NomeIngrediente = LimparNome(nomeIngrediente),
            TextoOriginal   = linha,
            Opcional        = opcional,
        };
    }

    // Converte "1/2", "1 1/2", "0.5", "1,5" em decimal.
    private static decimal? ParsearQuantidade(string texto)
    {
        texto = texto.Trim();
        if (texto.Length == 0) return null;

        var misto = Regex.Match(texto, @"^(\d+)\s+(\d+)/(\d+)$");
        if (misto.Success)
        {
            var inteiro = decimal.Parse(misto.Groups[1].Value, CultureInfo.InvariantCulture);
            var den = decimal.Parse(misto.Groups[3].Value, CultureInfo.InvariantCulture);
            return den == 0 ? inteiro
                : inteiro + decimal.Parse(misto.Groups[2].Value, CultureInfo.InvariantCulture) / den;
        }

        var fracao = Regex.Match(texto, @"^(\d+)/(\d+)$");
        if (fracao.Success)
        {
            var den = decimal.Parse(fracao.Groups[2].Value, CultureInfo.InvariantCulture);
            return den == 0 ? null
                : decimal.Parse(fracao.Groups[1].Value, CultureInfo.InvariantCulture) / den;
        }

        return decimal.TryParse(texto.Replace(",", "."), NumberStyles.Any,
            CultureInfo.InvariantCulture, out var q) ? q : null;
    }

    // Remove marcações de opcionalidade e pontuação residual do nome.
    private static string LimparNome(string nome)
    {
        nome = Regex.Replace(nome, @"\(?\s*\bopcional\b\s*\)?", " ", RegexOptions.IgnoreCase);
        nome = Regex.Replace(nome, @"\ba gosto\b", " ", RegexOptions.IgnoreCase);
        nome = Regex.Replace(nome, @"\s{2,}", " ");
        return nome.Trim().Trim(',', '.', ';', '-', ' ');
    }

    private static string LimparHtml(string texto)
        => Regex.Replace(texto, @"<[^>]+>", " ").Trim();
}
