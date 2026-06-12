using Application.HomePlanner.Repositories.Cardapio;
using Domain.HomePlanner.Models.Cardapio;
using Infrastructure.HomePlanner.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.HomePlanner.Repositories.Cardapio;

public class MarcacaoCompraRepository : IMarcacaoCompraRepository
{
    private readonly HomePlannerDbContext _db;
    private readonly IDbContextFactory<HomePlannerDbContext> _dbFactory;

    public MarcacaoCompraRepository(HomePlannerDbContext db, IDbContextFactory<HomePlannerDbContext> dbFactory)
    {
        _db = db;
        _dbFactory = dbFactory;
    }

    public async Task<IReadOnlyDictionary<int, bool>> ObterDaSemanaAsync(
        DateOnly dataInicio, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        // Filtro global de tenant já aplicado.
        return await db.MarcacoesCompra
            .Where(m => m.DataInicioSemana == dataInicio)
            .ToDictionaryAsync(m => m.IngredienteId, m => m.Comprado, ct);
    }

    public async Task<MarcacaoCompra?> ObterAsync(
        DateOnly dataInicio, int ingredienteId, CancellationToken ct = default)
        => await _db.MarcacoesCompra
            .FirstOrDefaultAsync(m => m.DataInicioSemana == dataInicio && m.IngredienteId == ingredienteId, ct);

    public async Task<IReadOnlyList<MarcacaoCompra>> ListarRastreadasDaSemanaAsync(
        DateOnly dataInicio, CancellationToken ct = default)
        => await _db.MarcacoesCompra
            .Where(m => m.DataInicioSemana == dataInicio)
            .ToListAsync(ct);

    public async Task AdicionarAsync(MarcacaoCompra entidade, CancellationToken ct = default)
        => await _db.MarcacoesCompra.AddAsync(entidade, ct);

    public Task<int> SalvarAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
