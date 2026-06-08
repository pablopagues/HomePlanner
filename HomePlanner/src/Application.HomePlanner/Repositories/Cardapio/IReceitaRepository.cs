using Application.HomePlanner.DTOs.Cardapio.Receita;
using Application.HomePlanner.Services.Cardapio;
using Domain.HomePlanner.Models.Cardapio;

namespace Application.HomePlanner.Repositories.Cardapio;

public interface IReceitaRepository
{
    Task<IReadOnlyList<ReceitaListaDTO>> ListarAsync(ReceitaFiltroDTO filtro, CancellationToken ct = default);
    Task<int> ContarAsync(ReceitaFiltroDTO filtro, CancellationToken ct = default);
    Task<ReceitaDetalheDTO?> ObterDetalheAsync(int id, CancellationToken ct = default);
    Task<Receita?> ObterEntidadeComIngredientesAsync(int id, CancellationToken ct = default);

    /// <summary>Carrega o grafo de receitas do tenant (porções base, ingredientes próprios e componentes).</summary>
    Task<GrafoReceitas> ObterGrafoReceitasAsync(CancellationToken ct = default);

    /// <summary>Nomes das receitas que usam <paramref name="componenteId"/> como componente ativo.</summary>
    Task<IReadOnlyList<string>> ObterPaisQueUsamAsync(int componenteId, CancellationToken ct = default);

    /// <summary>Receitas (autocomplete) por texto, excluindo opcionalmente um id.</summary>
    Task<IReadOnlyList<ReceitaListaDTO>> BuscarAutoCompleteAsync(string textoNormalizado, int limite, int? excluirId, CancellationToken ct = default);

    Task AdicionarAsync(Receita entidade, CancellationToken ct = default);
    Task<int> SalvarAsync(CancellationToken ct = default);
}
