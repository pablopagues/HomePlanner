using Application.HomePlanner.DTOs.Cardapio.Ingrediente;
using Domain.HomePlanner.Models.Cardapio;

namespace Application.HomePlanner.Repositories.Cardapio;

public interface IIngredienteRepository
{
    Task<IReadOnlyList<IngredienteListaDTO>> ListarAsync(IngredienteFiltroDTO filtro, CancellationToken ct = default);
    Task<int> ContarAsync(IngredienteFiltroDTO filtro, CancellationToken ct = default);
    Task<IngredienteDetalheDTO?> ObterDetalheAsync(int id, CancellationToken ct = default);
    Task<Ingrediente?> ObterEntidadeAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<IngredienteListaDTO>> BuscarAutoCompleteAsync(string textoNormalizado, int limite, CancellationToken ct = default);
    Task<IReadOnlyList<IngredienteListaDTO>> DetectarSimilaresAsync(string nomeNormalizado, int? excluirId, CancellationToken ct = default);
    Task<bool> ExisteNomeDuplicadoAsync(string nomeNormalizado, int? excluirId, CancellationToken ct = default);

    /// <summary>Resolve, para cada id, o produto base efetivo (id + nome) usado na lista de compras.</summary>
    Task<IReadOnlyDictionary<int, ProdutoBaseInfo>> ObterMapaBaseAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);

    /// <summary>Ingredientes (rastreados) que apontam diretamente para <paramref name="paiId"/> como base.</summary>
    Task<IReadOnlyList<Ingrediente>> ObterFilhosDiretosAsync(int paiId, CancellationToken ct = default);

    Task AdicionarAsync(Ingrediente entidade, CancellationToken ct = default);
    Task<int> SalvarAsync(CancellationToken ct = default);
}
