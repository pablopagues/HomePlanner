using Application.HomePlanner.Repositories.Cardapio;
using Domain.HomePlanner.Models.Cardapio;
using Infrastructure.HomePlanner.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.HomePlanner.Repositories.Cardapio;

public class ProdutoRecorrenteRepository : IProdutoRecorrenteRepository
{
    private readonly HomePlannerDbContext _db;
    private readonly IDbContextFactory<HomePlannerDbContext> _dbFactory;

    public ProdutoRecorrenteRepository(HomePlannerDbContext db, IDbContextFactory<HomePlannerDbContext> dbFactory)
    {
        _db = db;
        _dbFactory = dbFactory;
    }

    public async Task<IReadOnlyList<ProdutoRecorrente>> ListarAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        // Filtro global de tenant/soft-delete já aplicado.
        return await db.ProdutosRecorrentes
            .OrderBy(p => p.Ordem)
            .ThenBy(p => p.Descricao)
            .ToListAsync(ct);
    }

    public async Task<ProdutoRecorrente?> ObterEntidadeAsync(int id, CancellationToken ct = default)
        => await _db.ProdutosRecorrentes.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<ProdutoRecorrente>> ObterVariasAsync(
        IReadOnlyCollection<int> ids, CancellationToken ct = default)
    {
        if (ids.Count == 0) return [];
        return await _db.ProdutosRecorrentes
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(ct);
    }

    public async Task<int> ProximaOrdemAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var max = await db.ProdutosRecorrentes.Select(p => (int?)p.Ordem).MaxAsync(ct);
        return (max ?? -1) + 1;
    }

    public async Task AdicionarAsync(ProdutoRecorrente entidade, CancellationToken ct = default)
        => await _db.ProdutosRecorrentes.AddAsync(entidade, ct);

    public async Task<IReadOnlyList<string>> ListarDescricoesHistoricoAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        // Descrições distintas de pedidos passados que ainda não viraram recorrentes.
        return await db.PedidosCompra
            .Where(p => !db.ProdutosRecorrentes.Any(r => r.Descricao == p.Descricao))
            .Select(p => p.Descricao)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync(ct);
    }

    public Task<int> SalvarAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
