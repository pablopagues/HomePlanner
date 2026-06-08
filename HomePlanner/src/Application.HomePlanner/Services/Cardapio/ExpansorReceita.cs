using Application.HomePlanner.DTOs.Cardapio.Receita;

namespace Application.HomePlanner.Services.Cardapio;

/// <summary>
/// Grafo leve das receitas do tenant: porções base, ingredientes próprios (na
/// quantidade base) e arestas de componente. Alimenta o <see cref="ExpansorReceita"/>.
/// </summary>
public class GrafoReceitas
{
    public required IReadOnlyDictionary<int, int> PorcoesBase { get; init; }
    public required IReadOnlyDictionary<int, IReadOnlyList<IngredienteExpandidoDTO>> Ingredientes { get; init; }
    public required IReadOnlyDictionary<int, IReadOnlyList<ComponenteAresta>> Componentes { get; init; }
}

public readonly record struct ComponenteAresta(int ComponenteId, int PorcoesDesejadas);

/// <summary>
/// Expande recursivamente uma receita (própria + componentes) numa lista achatada
/// de ingredientes, escalando por porções. Função pura — não acessa o banco.
/// </summary>
public static class ExpansorReceita
{
    /// <summary>Expande a receita para <paramref name="porcoesAlvo"/> porções.</summary>
    public static List<IngredienteExpandidoDTO> Expandir(
        GrafoReceitas grafo, int receitaId, decimal porcoesAlvo)
    {
        var resultado = new List<IngredienteExpandidoDTO>();
        ExpandirInterno(grafo, receitaId, porcoesAlvo, new HashSet<int>(), resultado);
        return resultado;
    }

    private static void ExpandirInterno(
        GrafoReceitas grafo, int receitaId, decimal porcoesAlvo,
        HashSet<int> caminho, List<IngredienteExpandidoDTO> acc)
    {
        if (!grafo.PorcoesBase.TryGetValue(receitaId, out var porcoesBase)) return; // receita ausente
        if (!caminho.Add(receitaId)) return; // ciclo — interrompe este ramo

        var escala = porcoesAlvo / Math.Max(1, porcoesBase);

        if (grafo.Ingredientes.TryGetValue(receitaId, out var proprios))
        {
            foreach (var ing in proprios)
            {
                acc.Add(new IngredienteExpandidoDTO
                {
                    IngredienteId   = ing.IngredienteId,
                    NomeIngrediente = ing.NomeIngrediente,
                    Quantidade      = ing.Quantidade * escala,
                    UnidadeMedidaId = ing.UnidadeMedidaId,
                    CodigoUnidade   = ing.CodigoUnidade,
                    NomeUnidade     = ing.NomeUnidade,
                });
            }
        }

        if (grafo.Componentes.TryGetValue(receitaId, out var componentes))
        {
            foreach (var c in componentes)
                ExpandirInterno(grafo, c.ComponenteId, c.PorcoesDesejadas * escala, caminho, acc);
        }

        caminho.Remove(receitaId);
    }

    /// <summary>Soma linhas do mesmo ingrediente+unidade; ordena por nome.</summary>
    public static List<IngredienteExpandidoDTO> Consolidar(IEnumerable<IngredienteExpandidoDTO> linhas)
        => linhas
            .GroupBy(l => (l.IngredienteId, l.UnidadeMedidaId))
            .Select(g => new IngredienteExpandidoDTO
            {
                IngredienteId   = g.Key.IngredienteId,
                NomeIngrediente = g.First().NomeIngrediente,
                Quantidade      = g.Sum(x => x.Quantidade),
                UnidadeMedidaId = g.Key.UnidadeMedidaId,
                CodigoUnidade   = g.First().CodigoUnidade,
                NomeUnidade     = g.First().NomeUnidade,
            })
            .OrderBy(l => l.NomeIngrediente)
            .ToList();

    /// <summary>
    /// Detecta se incluir <paramref name="componenteId"/> como componente de
    /// <paramref name="receitaPaiId"/> criaria um ciclo (o componente já contém o pai
    /// na sua árvore, ou é o próprio pai).
    /// </summary>
    public static bool CriariaCiclo(GrafoReceitas grafo, int receitaPaiId, int componenteId)
    {
        if (receitaPaiId == componenteId) return true;
        var visitados = new HashSet<int>();
        return Alcanca(grafo, componenteId, receitaPaiId, visitados);
    }

    private static bool Alcanca(GrafoReceitas grafo, int origem, int alvo, HashSet<int> visitados)
    {
        if (origem == alvo) return true;
        if (!visitados.Add(origem)) return false;
        if (!grafo.Componentes.TryGetValue(origem, out var filhos)) return false;
        return filhos.Any(f => Alcanca(grafo, f.ComponenteId, alvo, visitados));
    }
}
