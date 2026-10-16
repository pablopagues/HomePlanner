using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Cardapio.Ingrediente;

namespace Application.HomePlanner.Services.Cardapio;

public interface IIngredienteService
{
    /// <summary>False para o papel Filho: só consulta o catálogo, não altera.</summary>
    Task<bool> PodeGerenciarAsync();

    Task<ResultadoListagem<IngredienteListaDTO>> ListarAsync(IngredienteFiltroDTO filtro, CancellationToken ct = default);
    Task<ResultadoOperacao<IngredienteDetalheDTO>> ObterAsync(int id, CancellationToken ct = default);
    Task<ResultadoOperacao<int>> SalvarAsync(IngredientePersistenciaDTO dto, CancellationToken ct = default);
    /// <summary>Soft-delete; falha se alguma receita ainda usa o ingrediente.</summary>
    Task<ResultadoOperacao> DeletarAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<string>> ListarCategoriasAsync(CancellationToken ct = default);
    Task<IReadOnlyList<IngredienteListaDTO>> BuscarAutoCompleteAsync(string texto, int limite = 10, CancellationToken ct = default);
    Task<IReadOnlyList<IngredienteListaDTO>> DetectarSimilaresAsync(string nome, int? excluirId = null, CancellationToken ct = default);

    /// <summary>
    /// Define (ou remove, com <paramref name="baseId"/> nulo) o produto base de compra de
    /// um ingrediente, achatando cadeias e re-apontando variantes existentes para a raiz.
    /// </summary>
    Task<ResultadoOperacao> DefinirProdutoBaseAsync(int ingredienteId, int? baseId, CancellationToken ct = default);
}
