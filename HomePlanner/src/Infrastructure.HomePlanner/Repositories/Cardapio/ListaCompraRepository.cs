using Application.HomePlanner.Repositories.Cardapio;
using Domain.HomePlanner.Models.Cardapio;
using Infrastructure.HomePlanner.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.HomePlanner.Repositories.Cardapio;

public class ListaCompraRepository : IListaCompraRepository
{
    private readonly HomePlannerDbContext _db;
    private readonly IDbContextFactory<HomePlannerDbContext> _dbFactory;

    public ListaCompraRepository(HomePlannerDbContext db, IDbContextFactory<HomePlannerDbContext> dbFactory)
    {
        _db = db;
        _dbFactory = dbFactory;
    }

    public async Task<IReadOnlyList<ListaCompra>> ListarAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        // Filtro global de tenant/soft-delete já aplicado.
        return await db.ListasCompra
            .OrderBy(l => l.Ordem)
            .ThenBy(l => l.Nome)
            .ToListAsync(ct);
    }

    public async Task<ListaCompra?> ObterEntidadeAsync(int id, CancellationToken ct = default)
        => await _db.ListasCompra.FirstOrDefaultAsync(l => l.Id == id, ct);

    public async Task<int> ProximaOrdemAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var max = await db.ListasCompra.Select(l => (int?)l.Ordem).MaxAsync(ct);
        return (max ?? -1) + 1;
    }

    public async Task AdicionarAsync(ListaCompra entidade, CancellationToken ct = default)
        => await _db.ListasCompra.AddAsync(entidade, ct);

    public async Task<IReadOnlyDictionary<int, int>> ObterPreferenciasAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        // Só preferências cuja lista ainda existe (não soft-deletada), via join com o filtro global.
        return await db.PreferenciasLojaIngrediente
            .Where(p => db.ListasCompra.Any(l => l.Id == p.ListaId))
            .ToDictionaryAsync(p => p.IngredienteId, p => p.ListaId, ct);
    }

    public async Task<PreferenciaLojaIngrediente?> ObterPreferenciaAsync(int ingredienteId, CancellationToken ct = default)
        => await _db.PreferenciasLojaIngrediente
            .FirstOrDefaultAsync(p => p.IngredienteId == ingredienteId, ct);

    public async Task AdicionarPreferenciaAsync(PreferenciaLojaIngrediente entidade, CancellationToken ct = default)
        => await _db.PreferenciasLojaIngrediente.AddAsync(entidade, ct);

    public void RemoverPreferencia(PreferenciaLojaIngrediente entidade)
        => _db.PreferenciasLojaIngrediente.Remove(entidade);

    public async Task RemoverPreferenciasDaListaAsync(int listaId, CancellationToken ct = default)
    {
        var prefs = await _db.PreferenciasLojaIngrediente
            .Where(p => p.ListaId == listaId)
            .ToListAsync(ct);
        _db.PreferenciasLojaIngrediente.RemoveRange(prefs);
    }

    public Task<int> SalvarAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
