using Domain.HomePlanner.Models.Cardapio;

namespace Application.HomePlanner.Repositories.Cardapio;

public interface IProdutoRecorrenteRepository
{
    /// <summary>Produtos recorrentes do tenant (não deletados), ordenados por Ordem e Descrição.</summary>
    Task<IReadOnlyList<ProdutoRecorrente>> ListarAsync(CancellationToken ct = default);
    Task<ProdutoRecorrente?> ObterEntidadeAsync(int id, CancellationToken ct = default);

    /// <summary>Carrega várias entidades por id (para envio em lote à semana).</summary>
    Task<IReadOnlyList<ProdutoRecorrente>> ObterVariasAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);

    /// <summary>Maior Ordem atual + 1 (para posicionar um novo produto no fim).</summary>
    Task<int> ProximaOrdemAsync(CancellationToken ct = default);
    Task AdicionarAsync(ProdutoRecorrente entidade, CancellationToken ct = default);

    /// <summary>
    /// Descrições distintas de pedidos avulsos já feitos no tenant que ainda NÃO estão
    /// no catálogo de recorrentes (para a importação em lote a partir do histórico).
    /// </summary>
    Task<IReadOnlyList<string>> ListarDescricoesHistoricoAsync(CancellationToken ct = default);

    Task<int> SalvarAsync(CancellationToken ct = default);
}
