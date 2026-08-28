using System.Globalization;
using Application.HomePlanner.Common;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Resources.HomePlanner;
using Xunit;

namespace Tests.HomePlanner;

/// <summary>
/// O código do erro é o contrato com os clientes; a tradução é o que o usuário lê.
/// Se um código não tiver chave, o usuário veria o texto padrão em português — ou, sem
/// a rede de segurança, o próprio código. Estes testes falham antes disso acontecer.
/// </summary>
public class ErrosAppTests
{
    private static IStringLocalizer<SharedResource> Localizer()
    {
        var options = Options.Create(new LocalizationOptions { ResourcesPath = "Resources" });
        var factory = new ResourceManagerStringLocalizerFactory(
            options, Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
        return new StringLocalizer<SharedResource>(factory);
    }

    [Theory]
    [InlineData("pt-BR")]
    [InlineData("en")]
    [InlineData("es")]
    [InlineData("fr")]
    public void Todo_codigo_tem_traducao(string cultura)
    {
        var anterior = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo(cultura);
            var loc = Localizer();

            var semTraducao = ErrosApp.TodosOsCodigos
                .Where(c => loc[$"Erro_{c}"].ResourceNotFound)
                .ToList();

            Assert.True(semTraducao.Count == 0,
                $"{semTraducao.Count} código(s) sem tradução em {cultura}: {string.Join(", ", semTraducao)}");
        }
        finally { CultureInfo.CurrentUICulture = anterior; }
    }

    [Fact]
    public void Catalogo_nao_esta_vazio()
    {
        // Guarda contra o teste de tradução passar à toa: se a reflexão parar de
        // enxergar as propriedades, TodosOsCodigos ficaria vazio e tudo "passaria".
        Assert.True(ErrosApp.TodosOsCodigos.Count >= 80,
            $"Catálogo com apenas {ErrosApp.TodosOsCodigos.Count} códigos — a reflexão provavelmente quebrou.");
    }

    [Fact]
    public void Erro_traduz_para_o_idioma_atual()
    {
        var anterior = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");
            Assert.Equal("Recette introuvable.", Localizer().Traduzir(ErrosApp.ReceitaNaoEncontrada));
        }
        finally { CultureInfo.CurrentUICulture = anterior; }
    }

    [Fact]
    public void Erro_com_argumento_preenche_o_placeholder()
    {
        var anterior = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("en");
            var texto = Localizer().Traduzir(ErrosApp.LimiteMembrosPlano(5));
            Assert.Contains("5", texto);
            Assert.DoesNotContain("{0}", texto);
        }
        finally { CultureInfo.CurrentUICulture = anterior; }
    }

    [Fact]
    public void Erro_externo_passa_o_texto_como_veio()
    {
        // Mensagem do ASP.NET Identity / Stripe: não temos código para ela.
        var erro = ErroOperacao.Externa("Passwords must have at least one digit.");
        Assert.Equal("Passwords must have at least one digit.", Localizer().Traduzir(erro));
    }

    [Fact]
    public void Codigo_sem_chave_cai_no_texto_padrao()
    {
        // Rede de segurança da migração: nunca mostrar o código cru na tela.
        var erro = ErroOperacao.De("codigo_que_nao_existe", "Texto de reserva.");
        Assert.Equal("Texto de reserva.", Localizer().Traduzir(erro));
    }

    [Fact]
    public void Resultado_com_texto_solto_vira_erro_externo()
    {
        // Os 170 pontos ainda não migrados continuam funcionando por este caminho.
        var r = ResultadoOperacao.Falha("Mensagem antiga.");
        Assert.Single(r.Erros);
        Assert.True(r.Erros[0].Externo);
        Assert.Equal("Mensagem antiga.", r.Erros[0].TextoPadrao);
    }
}
