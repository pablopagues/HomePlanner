using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Cardapio.Receita;

namespace Application.HomePlanner.Services.Cardapio;

public interface IReceitaService
{
    Task<ResultadoListagem<ReceitaListaDTO>> ListarAsync(ReceitaFiltroDTO filtro, CancellationToken ct = default);
    Task<ResultadoOperacao<ReceitaDetalheDTO>> ObterAsync(int id, CancellationToken ct = default);
    Task<ResultadoOperacao<int>> SalvarAsync(ReceitaPersistenciaDTO dto, CancellationToken ct = default);
    Task<ResultadoOperacao> DeletarAsync(int id, CancellationToken ct = default);
    Task<ResultadoOperacao<int>> DuplicarAsync(int id, CancellationToken ct = default);
}
