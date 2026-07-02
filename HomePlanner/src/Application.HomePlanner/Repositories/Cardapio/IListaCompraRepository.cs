using Domain.HomePlanner.Models.Cardapio;

namespace Application.HomePlanner.Repositories.Cardapio;

public interface IListaCompraRepository
{
    /// <summary>Listas/lojas do tenant (não deletadas), ordenadas por Ordem e Nome.</summary>
    Task<IReadOnlyList<ListaCompra>> ListarAsync(CancellationToken ct = default);
    Task<ListaCompra?> ObterEntidadeAsync(int id, CancellationToken ct = default);

    /// <summary>Maior Ordem atual + 1 (para posicionar uma nova lista no fim).</summary>
    Task<int> ProximaOrdemAsync(CancellationToken ct = default);
    Task AdicionarAsync(ListaCompra entidade, CancellationToken ct = default);

    /// <summary>Mapa IngredienteId → ListaId das preferências (loja padrão) do tenant.</summary>
    Task<IReadOnlyDictionary<int, int>> ObterPreferenciasAsync(CancellationToken ct = default);
    Task<PreferenciaLojaIngrediente?> ObterPreferenciaAsync(int ingredienteId, CancellationToken ct = default);
    Task AdicionarPreferenciaAsync(PreferenciaLojaIngrediente entidade, CancellationToken ct = default);
    void RemoverPreferencia(PreferenciaLojaIngrediente entidade);

    /// <summary>Remove todas as preferências que apontam para uma lista (ao excluí-la).</summary>
    Task RemoverPreferenciasDaListaAsync(int listaId, CancellationToken ct = default);

    Task<int> SalvarAsync(CancellationToken ct = default);
}
