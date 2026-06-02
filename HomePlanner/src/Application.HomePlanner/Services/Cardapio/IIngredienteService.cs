using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Cardapio.Ingrediente;

namespace Application.HomePlanner.Services.Cardapio;

public interface IIngredienteService
{
    Task<ResultadoListagem<IngredienteListaDTO>> ListarAsync(IngredienteFiltroDTO filtro, CancellationToken ct = default);
    Task<ResultadoOperacao<IngredienteDetalheDTO>> ObterAsync(int id, CancellationToken ct = default);
    Task<ResultadoOperacao<int>> SalvarAsync(IngredientePersistenciaDTO dto, CancellationToken ct = default);
    Task<ResultadoOperacao> DeletarAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<IngredienteListaDTO>> BuscarAutoCompleteAsync(string texto, int limite = 10, CancellationToken ct = default);
    Task<IReadOnlyList<IngredienteListaDTO>> DetectarSimilaresAsync(string nome, int? excluirId = null, CancellationToken ct = default);
}
