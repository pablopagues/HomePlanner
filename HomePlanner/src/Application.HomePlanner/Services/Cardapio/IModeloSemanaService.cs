using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Cardapio.Cardapio;
using Application.HomePlanner.DTOs.Cardapio.ModeloSemana;

namespace Application.HomePlanner.Services.Cardapio;

public interface IModeloSemanaService
{
    Task<ResultadoListagem<ModeloSemanaListaDTO>> ListarAsync(ModeloSemanaFiltroDTO filtro, CancellationToken ct = default);
    Task<ResultadoOperacao<ModeloSemanaDetalheDTO>> ObterAsync(int id, CancellationToken ct = default);
    Task<ResultadoOperacao<int>> SalvarAsync(ModeloSemanaPersistenciaDTO dto, CancellationToken ct = default);
    Task<ResultadoOperacao> DeletarAsync(int id, CancellationToken ct = default);
    Task<ResultadoOperacao<CardapioSemanaDTO>> AplicarModeloAsync(int modeloId, DateOnly dataInicio, CancellationToken ct = default);
    Task<ResultadoOperacao<int>> SalvarPlanejamentoComoModeloAsync(DateOnly dataInicio, string nomeModelo, CancellationToken ct = default);
}
