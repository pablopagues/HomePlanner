using Application.HomePlanner.DTOs.Cardapio.ModeloSemana;
using Domain.HomePlanner.Models.Cardapio;

namespace Application.HomePlanner.Repositories.Cardapio;

public interface IModeloSemanaRepository
{
    Task<IReadOnlyList<ModeloSemanaListaDTO>> ListarAsync(ModeloSemanaFiltroDTO filtro, CancellationToken ct = default);
    Task<int> ContarAsync(ModeloSemanaFiltroDTO filtro, CancellationToken ct = default);
    Task<ModeloSemanaDetalheDTO?> ObterDetalheAsync(int id, CancellationToken ct = default);
    Task<ModeloSemana?> ObterEntidadeComRefeicoesAsync(int id, CancellationToken ct = default);
    Task AdicionarAsync(ModeloSemana entidade, CancellationToken ct = default);
    Task<int> SalvarAsync(CancellationToken ct = default);
}
