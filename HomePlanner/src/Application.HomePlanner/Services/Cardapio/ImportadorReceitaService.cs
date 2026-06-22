using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Cardapio.Receita;
using Application.HomePlanner.Helpers;
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
    private readonly IParserIngredientesIA _parserIA;
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
        // Colher isolada / plural / sem acento (default = sopa).
        ["colher"] = "cs", ["colheres"] = "cs", ["colheres de sopa"] = "cs",
        ["colher de cha"] = "cc", ["colheres de chá"] = "cc", ["colheres de cha"] = "cc",
        ["colher de café"] = "cc", ["colher de cafe"] = "cc",
        // Xícara plural; copo aproximado de volume.
        ["xícaras"] = "xic", ["xicaras"] = "xic",
        ["copo"] = "xic", ["copos"] = "xic",
        // Embalagens / contáveis tratados como unidade.
        ["lata"] = "un", ["latas"] = "un", ["caixa"] = "un", ["caixas"] = "un",
        ["vidro"] = "un", ["vidros"] = "un", ["pote"] = "un", ["potes"] = "un",
        ["ramo"] = "un", ["ramos"] = "un", ["folha"] = "un", ["folhas"] = "un",
        ["talo"] = "un", ["talos"] = "un",
    };

    // Palavras-chave de proteína/corte (sem acento — comparadas contra texto
    // normalizado): produtos distintos no mercado, o nome é mantido intacto
    // (não separamos termos de preparo como "em cubos").
    private static readonly string[] _palavrasProteina =
    {
        "carne", "bife", "file", "frango", "peito", "coxa", "sobrecoxa",
        "picanha", "alcatra", "patinho", "maminha", "costela", "lombo", "pernil",
        "peixe", "salmao", "tilapia", "linguica", "bacon", "presunto",
        "moida", "moido", "acem", "fraldinha", "cupim", "panceta", "bisteca",
        "mignon", "musculo",
    };

    // Descritores de preparo/tamanho/estado (sem acento) que, em itens não-cárneos,
    // são movidos para a observação (compra-se o produto inteiro e depois transforma).
    private static readonly string[] _descritoresPreparo =
    {
        "picad", "ralad", "cortad", "fatiad", "picadinh", "amassad", "esmagad",
        "batid", "derretid", "peneirad", "triturad", "descascad", "lavad", "ralinh",
        "grande", "medio", "media", "medios", "medias", "pequen", "grosso", "grossa",
        "fino", "fina", "finamente", "bem", "quente", "morna", "morno", "gelad",
        "fria", "frio", "fervente", "fervendo",
    };

    public ImportadorReceitaService(
        HttpClient http,
        IUnidadeMedidaRepository unidadeRepo,
        IParserIngredientesIA parserIA,
        ILogger<ImportadorReceitaService> logger)
    {
        _http = http;
        _unidadeRepo = unidadeRepo;
        _parserIA = parserIA;
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
        => SepararLinhas(texto).Select(ParsearLinhaIngrediente).ToList();

    public async Task<IReadOnlyList<IngredienteImportadoDTO>> ParsearTextoAsync(
        string? texto, CancellationToken ct = default)
        => await ParsearLinhasAsync(SepararLinhas(texto), ct);

    // Quebra um texto livre de ingredientes em linhas limpas (sem marcadores/cabeçalhos).
    private static IReadOnlyList<string> SepararLinhas(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return [];

        var linhas = new List<string>();
        foreach (var bruta in texto.Split('\n'))
        {
            var linha = bruta.Trim().TrimStart('-', '•', '*', '·', '–', '—', ' ').Trim();
            if (linha.Length == 0 || EhCabecalho(linha)) continue;
            linhas.Add(linha);
        }
        return linhas;
    }

    // Parseia linhas via IA (qualquer idioma); cai no regex se a IA falhar/estiver desligada.
    private async Task<IReadOnlyList<IngredienteImportadoDTO>> ParsearLinhasAsync(
        IReadOnlyList<string> linhas, CancellationToken ct)
    {
        if (linhas.Count == 0) return [];

        if (_parserIA.Habilitado)
        {
            var ia = await _parserIA.ParsearAsync(linhas, ct);
            if (ia is not null)
            {
                _logger.LogInformation(
                    "Ingredientes parseados via IA (Claude): {Total} de {Linhas} linha(s).",
                    ia.Count, linhas.Count);
                return ia;
            }
            _logger.LogInformation("Parser IA habilitado, mas retornou nulo — usando regex de fallback.");
        }
        else
        {
            _logger.LogInformation("Parser IA desabilitado (sem API key) — usando regex.");
        }
        return linhas.Select(ParsearLinhaIngrediente).ToList();
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

                // Objeto simples: só retorna se for de fato uma Recipe; caso contrário
                // continua para o próximo bloco (sites põem Article/WebPage/Organization
                // antes do bloco da receita).
                var simples = TentarMapearReceita(raiz, urlOrigem);
                if (simples is not null) return simples;
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
        var titulo = LerMetaOpenGraph(html, "og:title");
        if (string.IsNullOrWhiteSpace(titulo))
            titulo = Regex.Match(html, @"<title[^>]*>([^<]+)</title>", RegexOptions.IgnoreCase).Groups[1].Value.Trim();
        if (string.IsNullOrWhiteSpace(titulo)) return null;

        var imagem = LerMetaOpenGraph(html, "og:image");

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

    // Lê uma meta Open Graph aceitando as duas ordens de atributo:
    // <meta property="og:x" content="..."> ou <meta content="..." property="og:x">.
    private static string LerMetaOpenGraph(string html, string propriedade)
    {
        var prop = Regex.Escape(propriedade);
        var depois = Regex.Match(html,
            $@"{prop}[""'][^>]*?content=[""']([^""']+)[""']", RegexOptions.IgnoreCase);
        if (depois.Success) return depois.Groups[1].Value.Trim();

        var antes = Regex.Match(html,
            $@"content=[""']([^""']+)[""'][^>]*?(?:property|name)=[""']{prop}[""']", RegexOptions.IgnoreCase);
        return antes.Success ? antes.Groups[1].Value.Trim() : string.Empty;
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
        var original = LimparHtml(linha).Trim();
        linha = original;

        // "(opcional)", "opcional" ou "a gosto" → marca como opcional.
        bool opcional = Regex.IsMatch(linha, @"\bopcional\b|\ba gosto\b", RegexOptions.IgnoreCase);

        // Remove conjunção isolada no início ("e 1/2 litro..." → "1/2 litro...").
        linha = Regex.Replace(linha, @"^(?:e|ou)\s+", "", RegexOptions.IgnoreCase);

        // Normaliza unidades brasileiras escritas com parênteses ("colher (sopa)").
        linha = NormalizarUnidadesParentizadas(linha);

        // 1) Quantidade no início: número, fração, mista ou faixa ("1 a 2", "2-3").
        decimal? quantidade = null;
        // Ordem importa: mista ("1 1/2") e fração ("1/2") antes do inteiro/decimal,
        // senão "1/2" casaria só o "1".
        var mQtd = Regex.Match(linha,
            @"^(\d+\s+\d+/\d+|\d+/\d+|\d+(?:[.,]\d+)?)(?:\s*(?:a|-|–|—|até)\s*\d+(?:[.,]\d+)?)?\s*",
            RegexOptions.IgnoreCase);
        if (mQtd.Success && mQtd.Length > 0)
        {
            quantidade = ParsearQuantidade(mQtd.Groups[1].Value);
            linha = linha[mQtd.Length..].TrimStart();
        }

        // 2) Unidade logo após a quantidade (testa "colher de sopa" antes de "colher").
        string? codigoUnidade = null;
        if (quantidade is not null)
        {
            var (codigo, consumido) = ExtrairUnidade(linha);
            if (codigo is not null)
            {
                codigoUnidade = codigo;
                linha = linha[consumido..].TrimStart();
            }
        }

        // Remove "de "/"da "/"do " residual no começo do nome.
        linha = Regex.Replace(linha, @"^(?:de|da|do|d['’])\s+", "", RegexOptions.IgnoreCase);

        // 3) Separa o nome núcleo do termo de preparo.
        var (nome, preparo) = SepararNucleoEPreparo(linha);

        // Há quantidade mas nenhuma unidade reconhecida → item contável: "un".
        if (quantidade is not null && codigoUnidade is null)
            codigoUnidade = "un";

        var preparoLimpo = string.IsNullOrWhiteSpace(preparo) ? null : LimparNome(preparo);

        return new IngredienteImportadoDTO
        {
            Quantidade      = quantidade,
            CodigoUnidade   = codigoUnidade,
            NomeIngrediente = LimparNome(nome),
            Preparo         = string.IsNullOrWhiteSpace(preparoLimpo) ? null : preparoLimpo,
            TextoOriginal   = original,
            Opcional        = opcional,
        };
    }

    // "colher (sopa)" → "colher de sopa"; "xícara (chá)" → "xícara"; etc.
    private static string NormalizarUnidadesParentizadas(string texto)
    {
        texto = Regex.Replace(texto, @"\bcolher(es)?\s*\(\s*sopa\s*\)",
            "colher$1 de sopa", RegexOptions.IgnoreCase);
        texto = Regex.Replace(texto, @"\bcolher(es)?\s*\(\s*(?:ch[aá]|caf[eé])\s*\)",
            "colher$1 de chá", RegexOptions.IgnoreCase);
        texto = Regex.Replace(texto, @"\b(x[ií]cara?s?)\s*\(\s*(?:ch[aá]|caf[eé])\s*\)",
            "$1", RegexOptions.IgnoreCase);
        texto = Regex.Replace(texto, @"\b(copos?)\s*\([^)]*\)",
            "$1", RegexOptions.IgnoreCase);
        return texto;
    }

    // Extrai a unidade do início do texto, testando frases de 3, 2 e 1 token(s).
    // Retorna o código interno e quantos caracteres consumir.
    private static (string? codigo, int consumido) ExtrairUnidade(string texto)
    {
        var tokens = Regex.Matches(texto, @"\S+");
        for (int n = Math.Min(3, tokens.Count); n >= 1; n--)
        {
            var ultimo = tokens[n - 1];
            int fim = ultimo.Index + ultimo.Length;
            var chave = Regex.Replace(texto[..fim], @"\s+", " ").Trim();
            if (_mapaUnidades.TryGetValue(chave, out var codigo))
                return (codigo, fim);
        }
        return (null, 0);
    }

    // Separa "cebolas médias em cubos" → ("cebolas", "médias em cubos").
    // Cortes/produtos cárneos ficam intactos (o corte É o produto comprado).
    private static (string nome, string? preparo) SepararNucleoEPreparo(string frase)
    {
        frase = frase.Trim();
        if (frase.Length == 0) return (frase, null);

        var normalizada = TextoHelper.NormalizarNome(frase);
        if (_palavrasProteina.Any(p => Regex.IsMatch(normalizada, $@"\b{p}")))
            return (frase, null);

        var tokens = frase.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int corte = -1;
        for (int i = 0; i < tokens.Length; i++)
        {
            var t = TextoHelper.NormalizarNome(tokens[i]);
            // "em cubos", "em rodelas"... — preposição "em" seguida de palavra.
            if (t == "em" && i + 1 < tokens.Length) { corte = i; break; }
            if (_descritoresPreparo.Any(d => t.StartsWith(d))) { corte = i; break; }
        }

        // corte <= 0: nada a separar, ou o primeiro token já é descritor (mantém tudo).
        if (corte <= 0) return (frase, null);

        var nome    = string.Join(' ', tokens[..corte]).Trim();
        var preparo = string.Join(' ', tokens[corte..]).Trim();
        return (nome.Length == 0 ? frase : nome, preparo);
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
