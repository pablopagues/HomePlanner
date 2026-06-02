using Application.HomePlanner.DTOs.Cardapio.UnidadeMedida;
using Application.HomePlanner.Repositories.Cardapio;
using Domain.HomePlanner.Models.Cardapio;
using Infrastructure.HomePlanner.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.HomePlanner.Repositories.Cardapio;

public class UnidadeMedidaRepository : IUnidadeMedidaRepository
{
    private readonly IDbContextFactory<HomePlannerDbContext> _dbFactory;

    public UnidadeMedidaRepository(IDbContextFactory<HomePlannerDbContext> dbFactory)
        => _dbFactory = dbFactory;

    public async Task<IReadOnlyList<UnidadeMedidaListaDTO>> ListarAtivasAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.UnidadesMedida
            .Where(u => u.IsAtivo)
            .OrderBy(u => u.Tipo).ThenBy(u => u.Nome)
            .Select(u => new UnidadeMedidaListaDTO
            {
                Id            = u.Id,
                Codigo        = u.Codigo,
                Nome          = u.Nome,
                ChaveTraducao = u.ChaveTraducao,
                Tipo          = u.Tipo,
                FatorParaBase = u.FatorParaBase,
            })
            .ToListAsync(ct);
    }

    public async Task<UnidadeMedida?> ObterPorIdAsync(int id, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.UnidadesMedida.FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<UnidadeMedida?> ObterPorCodigoAsync(string codigo, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.UnidadesMedida
            .FirstOrDefaultAsync(u => u.Codigo == codigo && u.IsAtivo, ct);
    }
}
