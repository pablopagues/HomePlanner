using Application.HomePlanner.DTOs.Cardapio.Receita;
using Application.HomePlanner.Services.Cardapio;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Tests.HomePlanner;

/// <summary>
/// Cobre o parser de texto livre de ingredientes. <c>ParsearTexto</c> só usa
/// helpers internos, então as dependências do construtor não são exercitadas.
/// </summary>
public class ImportadorReceitaParserTests
{
    private static IReadOnlyList<IngredienteImportadoDTO> Parse(string linha)
    {
        var svc = new ImportadorReceitaService(
            http: null!, unidadeRepo: null!, parserIA: null!,
            logger: NullLogger<ImportadorReceitaService>.Instance);
        return svc.ParsearTexto(linha);
    }

    private static IngredienteImportadoDTO ParseUm(string linha)
    {
        var itens = Parse(linha);
        Assert.Single(itens);
        return itens[0];
    }

    [Fact]
    public void Quantidade_sem_unidade_vira_contavel_un()
    {
        var i = ParseUm("1 cebola");
        Assert.Equal(1m, i.Quantidade);
        Assert.Equal("un", i.CodigoUnidade);
        Assert.Equal("cebola", i.NomeIngrediente, ignoreCase: true);
        Assert.Null(i.Preparo);
    }

    [Fact]
    public void Separa_preparo_do_nucleo_em_hortalica()
    {
        var i = ParseUm("2 cebolas médias cortadas em cubos");
        Assert.Equal(2m, i.Quantidade);
        Assert.Equal("un", i.CodigoUnidade);
        Assert.Equal("cebolas", i.NomeIngrediente, ignoreCase: true);
        Assert.NotNull(i.Preparo);
        Assert.Contains("cubos", i.Preparo!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Reconhece_colher_sopa_entre_parenteses()
    {
        var i = ParseUm("1 colher (sopa) de óleo");
        Assert.Equal(1m, i.Quantidade);
        Assert.Equal("cs", i.CodigoUnidade);
        Assert.Equal("óleo", i.NomeIngrediente, ignoreCase: true);
    }

    [Fact]
    public void Reconhece_xicara_cha_entre_parenteses()
    {
        var i = ParseUm("1 xícara (chá) de água");
        Assert.Equal(1m, i.Quantidade);
        Assert.Equal("xic", i.CodigoUnidade);
        Assert.Equal("água", i.NomeIngrediente, ignoreCase: true);
    }

    [Fact]
    public void Cortes_de_carne_ficam_intactos()
    {
        var i = ParseUm("500 g de filé de frango sem pele cortado em cubos");
        Assert.Equal(500m, i.Quantidade);
        Assert.Equal("g", i.CodigoUnidade);
        Assert.Equal("filé de frango sem pele cortado em cubos", i.NomeIngrediente, ignoreCase: true);
        Assert.Null(i.Preparo);
    }

    [Fact]
    public void Reconhece_unidade_dente_e_separa_preparo()
    {
        var i = ParseUm("2 dentes de alho amassados");
        Assert.Equal(2m, i.Quantidade);
        Assert.Equal("dente", i.CodigoUnidade);
        Assert.Equal("alho", i.NomeIngrediente, ignoreCase: true);
        Assert.Contains("amassad", i.Preparo!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Remove_conjuncao_inicial_e_parseia_fracao()
    {
        var i = ParseUm("E 1/2 litro de água fervente");
        Assert.Equal(0.5m, i.Quantidade);
        Assert.Equal("l", i.CodigoUnidade);
        Assert.Equal("água", i.NomeIngrediente, ignoreCase: true);
    }

    [Fact]
    public void Item_sem_quantidade_nao_inventa_unidade()
    {
        var i = ParseUm("Sal a gosto");
        Assert.Null(i.Quantidade);
        Assert.Null(i.CodigoUnidade);
        Assert.Equal("sal", i.NomeIngrediente, ignoreCase: true);
        Assert.True(i.Opcional);
    }
}
