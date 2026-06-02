using Application.HomePlanner.DTOs.Cardapio.Receita;
using Domain.HomePlanner.Models.Cardapio;

namespace Application.HomePlanner.Repositories.Cardapio;

public interface IReceitaRepository
{
    Task<IReadOnlyList<ReceitaListaDTO>> ListarAsync(ReceitaFiltroDTO filtro, CancellationToken ct = default);
    Task<int> ContarAsync(ReceitaFiltroDTO filtro, CancellationToken ct = default);
    Task<ReceitaDetalheDTO?> ObterDetalheAsync(int id, CancellationToken ct = default);
    Task<Receita?> ObterEntidadeComIngredientesAsync(int id, CancellationToken ct = default);
    Task AdicionarAsync(Receita entidade, CancellationToken ct = default);
    Task<int> SalvarAsync(CancellationToken ct = default);
}
