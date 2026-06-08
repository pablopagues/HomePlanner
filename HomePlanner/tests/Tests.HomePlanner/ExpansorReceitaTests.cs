using Application.HomePlanner.DTOs.Cardapio.Receita;
using Application.HomePlanner.Services.Cardapio;
using Xunit;

namespace Tests.HomePlanner;

public class ExpansorReceitaTests
{
    // Monta um grafo a partir de tuplas simples.
    private static GrafoReceitas Grafo(
        (int Id, int PorcoesBase)[] receitas,
        (int ReceitaId, int IngredienteId, decimal Qtd, int UnId)[] ingredientes,
        (int PaiId, int CompId, int Porcoes)[] componentes)
        => new()
        {
            PorcoesBase = receitas.ToDictionary(r => r.Id, r => r.PorcoesBase),
            Ingredientes = ingredientes
                .GroupBy(i => i.ReceitaId)
                .ToDictionary(
                    g => g.Key,
                    g => (IReadOnlyList<IngredienteExpandidoDTO>)g.Select(i => new IngredienteExpandidoDTO
                    {
                        IngredienteId   = i.IngredienteId,
                        NomeIngrediente = $"Ing{i.IngredienteId}",
                        Quantidade      = i.Qtd,
                        UnidadeMedidaId = i.UnId,
                        CodigoUnidade   = "un",
                        NomeUnidade     = "Unidade",
                    }).ToList()),
            Componentes = componentes
                .GroupBy(c => c.PaiId)
                .ToDictionary(
                    g => g.Key,
                    g => (IReadOnlyList<ComponenteAresta>)g
                        .Select(c => new ComponenteAresta(c.CompId, c.Porcoes)).ToList()),
        };

    [Fact]
    public void Escala_ingredientes_proprios_pela_porcao_alvo()
    {
        var grafo = Grafo(
            receitas: [(1, 4)],
            ingredientes: [(1, 10, 200m, 1)],
            componentes: []);

        var r = ExpansorReceita.Expandir(grafo, receitaId: 1, porcoesAlvo: 8);

        Assert.Single(r);
        Assert.Equal(400m, r[0].Quantidade); // 200 * (8/4)
    }

    [Fact]
    public void Expande_componentes_nas_proprias_porcoes()
    {
        // Prato 1 (base 4) usa componente 2 (base 2) com 4 porções.
        var grafo = Grafo(
            receitas: [(1, 4), (2, 2)],
            ingredientes: [(1, 10, 100m, 1), (2, 20, 50m, 1)],
            componentes: [(1, 2, 4)]);

        var r = ExpansorReceita.Expandir(grafo, 1, 4);

        // Ingrediente próprio 10: 100 * (4/4) = 100
        Assert.Equal(100m, r.Single(x => x.IngredienteId == 10).Quantidade);
        // Componente: 50 * (4/2) = 100
        Assert.Equal(100m, r.Single(x => x.IngredienteId == 20).Quantidade);
    }

    [Fact]
    public void Escala_do_prato_propaga_para_componentes()
    {
        // Pede 8 porções de um prato base 4 → componente escala junto (×2).
        var grafo = Grafo(
            receitas: [(1, 4), (2, 2)],
            ingredientes: [(2, 20, 50m, 1)],
            componentes: [(1, 2, 4)]);

        var r = ExpansorReceita.Expandir(grafo, 1, 8);

        // escala do pai = 8/4 = 2; porções do componente = 4*2 = 8; qtd = 50*(8/2) = 200
        Assert.Equal(200m, r.Single(x => x.IngredienteId == 20).Quantidade);
    }

    [Fact]
    public void Aninhamento_em_dois_niveis()
    {
        // 1 → 2 → 3
        var grafo = Grafo(
            receitas: [(1, 4), (2, 4), (3, 4)],
            ingredientes: [(3, 30, 10m, 1)],
            componentes: [(1, 2, 4), (2, 3, 4)]);

        var r = ExpansorReceita.Expandir(grafo, 1, 4);

        Assert.Equal(10m, r.Single(x => x.IngredienteId == 30).Quantidade);
    }

    [Fact]
    public void Ciclo_nao_causa_loop_infinito()
    {
        // 1 → 2 → 1 (ciclo)
        var grafo = Grafo(
            receitas: [(1, 4), (2, 4)],
            ingredientes: [(1, 10, 5m, 1), (2, 20, 7m, 1)],
            componentes: [(1, 2, 4), (2, 1, 4)]);

        var r = ExpansorReceita.Expandir(grafo, 1, 4);

        // Não estoura; cada ingrediente aparece ao menos uma vez.
        Assert.Contains(r, x => x.IngredienteId == 10);
        Assert.Contains(r, x => x.IngredienteId == 20);
    }

    [Fact]
    public void Consolidar_soma_mesmo_ingrediente_e_unidade()
    {
        var linhas = new[]
        {
            new IngredienteExpandidoDTO { IngredienteId = 10, NomeIngrediente = "Cebola", Quantidade = 2, UnidadeMedidaId = 9, CodigoUnidade = "un" },
            new IngredienteExpandidoDTO { IngredienteId = 10, NomeIngrediente = "Cebola", Quantidade = 3, UnidadeMedidaId = 9, CodigoUnidade = "un" },
        };

        var r = ExpansorReceita.Consolidar(linhas);

        Assert.Single(r);
        Assert.Equal(5m, r[0].Quantidade);
    }

    [Fact]
    public void CriariaCiclo_detecta_auto_referencia_e_caminho()
    {
        // 2 já usa 1 como componente; 3 é isolada.
        var grafo = Grafo(
            receitas: [(1, 4), (2, 4), (3, 4)],
            ingredientes: [],
            componentes: [(2, 1, 4)]);

        Assert.True(ExpansorReceita.CriariaCiclo(grafo, 1, 1));   // auto-referência
        Assert.True(ExpansorReceita.CriariaCiclo(grafo, 1, 2));   // 1→2 fecharia o 2→1 existente
        Assert.False(ExpansorReceita.CriariaCiclo(grafo, 3, 1));  // 1 não alcança 3 → sem ciclo
    }
}
