using Application.HomePlanner.DTOs.Cardapio.UnidadeMedida;
using Domain.HomePlanner.Models.Cardapio;

namespace Application.HomePlanner.Repositories.Cardapio;

public interface IUnidadeMedidaRepository
{
    Task<IReadOnlyList<UnidadeMedidaListaDTO>> ListarAtivasAsync(CancellationToken ct = default);
    Task<UnidadeMedida?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<UnidadeMedida?> ObterPorCodigoAsync(string codigo, CancellationToken ct = default);
}
