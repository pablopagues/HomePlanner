using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Cardapio.Receita;

namespace Application.HomePlanner.Services.Cardapio;

public interface IReceitaService
{
    Task<ResultadoListagem<ReceitaListaDTO>> ListarAsync(ReceitaFiltroDTO filtro, CancellationToken ct = default);
    Task<ResultadoOperacao<ReceitaDetalheDTO>> ObterAsync(int id, CancellationToken ct = default);

    /// <summary>Conteúdo da foto da receita para ser servido por um endpoint, ou null se não houver.</summary>
    Task<ReceitaFotoDTO?> ObterFotoAsync(int id, CancellationToken ct = default);
    Task<ResultadoOperacao<int>> SalvarAsync(ReceitaPersistenciaDTO dto, CancellationToken ct = default);
    Task<ResultadoOperacao> DeletarAsync(int id, CancellationToken ct = default);
    Task<ResultadoOperacao<int>> DuplicarAsync(int id, CancellationToken ct = default);

    /// <summary>Receitas por texto (autocomplete de componentes), excluindo opcionalmente um id.</summary>
    Task<IReadOnlyList<ReceitaListaDTO>> BuscarAutoCompleteAsync(string texto, int limite = 10, int? excluirId = null, CancellationToken ct = default);

    /// <summary>Expande os componentes informados (cada um nas suas porções) em ingredientes consolidados.</summary>
    Task<IReadOnlyList<IngredienteExpandidoDTO>> ExpandirComponentesAsync(IReadOnlyList<ReceitaComponentePersistenciaDTO> componentes, CancellationToken ct = default);
}
