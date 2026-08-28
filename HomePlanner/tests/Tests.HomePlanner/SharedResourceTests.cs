using System.Globalization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Resources.HomePlanner;
using Xunit;

namespace Tests.HomePlanner;

/// <summary>
/// Trava a resolução das .resx compartilhadas entre a web e o app.
///
/// Quando o caminho do recurso e o namespace do marcador não batem, o IStringLocalizer
/// devolve a própria chave — sem exceção, sem aviso de compilação. A tela só aparece
/// com "Assin_BloqAcao" escrito na tela. Estes testes falham no lugar disso.
/// </summary>
public class SharedResourceTests
{
    private static IStringLocalizer<SharedResource> Localizer()
    {
        var options = Options.Create(new LocalizationOptions { ResourcesPath = "Resources" });
        var factory = new ResourceManagerStringLocalizerFactory(
            options, Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
        return new StringLocalizer<SharedResource>(factory);
    }

    private static string Traduzir(string chave, string cultura)
    {
        var anterior = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo(cultura);
            return Localizer()[chave].Value;
        }
        finally
        {
            CultureInfo.CurrentUICulture = anterior;
        }
    }

    [Theory]
    [InlineData("pt-BR")]
    [InlineData("en")]
    [InlineData("es")]
    [InlineData("fr")]
    public void Recurso_resolve_nos_quatro_idiomas(string cultura)
    {
        var valor = Traduzir("Assin_BloqAcao", cultura);

        // Devolver a própria chave é o sintoma de recurso não encontrado.
        Assert.NotEqual("Assin_BloqAcao", valor);
        Assert.False(string.IsNullOrWhiteSpace(valor));
    }

    [Fact]
    public void Idiomas_tem_traducoes_distintas()
    {
        // Se os satélites não carregarem, todos caem no neutro e ficam iguais.
        var pt = Traduzir("Assin_BloqAcao", "pt-BR");
        var en = Traduzir("Assin_BloqAcao", "en");
        var es = Traduzir("Assin_BloqAcao", "es");
        var fr = Traduzir("Assin_BloqAcao", "fr");

        Assert.Equal("Escolher plano", pt);
        Assert.Equal("Choose a plan", en);
        Assert.Equal("Elegir plan", es);
        Assert.Equal("Choisir une formule", fr);
    }

    [Theory]
    [InlineData("en")]
    [InlineData("es")]
    [InlineData("fr")]
    public void Nenhuma_chave_fica_sem_traducao(string cultura)
    {
        // tryParents: false devolve só o que o satélite daquele idioma traz. Uma chave
        // ausente aqui cairia no português sem ninguém perceber, num idioma inteiro.
        var rm = new System.Resources.ResourceManager(
            $"{typeof(SharedResource).Assembly.GetName().Name}.Resources.SharedResource",
            typeof(SharedResource).Assembly);

        var neutro = rm.GetResourceSet(CultureInfo.InvariantCulture, true, false)!;
        var idioma = rm.GetResourceSet(new CultureInfo(cultura), true, false);

        Assert.NotNull(idioma);

        var faltando = neutro.Cast<System.Collections.DictionaryEntry>()
            .Select(e => (string)e.Key)
            .Where(chave => idioma!.GetString(chave) is null)
            .OrderBy(c => c)
            .ToList();

        Assert.True(faltando.Count == 0,
            $"{faltando.Count} chave(s) sem tradução em {cultura}: {string.Join(", ", faltando.Take(15))}");
    }

    [Fact]
    public void Cultura_desconhecida_cai_no_portugues()
    {
        // .resx neutro é pt-BR — é o fallback quando o idioma não é suportado.
        Assert.Equal("Escolher plano", Traduzir("Assin_BloqAcao", "de"));
    }
}
