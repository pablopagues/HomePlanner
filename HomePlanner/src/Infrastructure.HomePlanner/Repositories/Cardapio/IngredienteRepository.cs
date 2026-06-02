using Application.HomePlanner.DTOs.Cardapio.Ingrediente;
using Application.HomePlanner.Repositories.Cardapio;
using Domain.HomePlanner.Models.Cardapio;
using Infrastructure.HomePlanner.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.HomePlanner.Repositories.Cardapio;

public class IngredienteRepository : IIngredienteRepository
{
    private readonly HomePlannerDbContext _db;
    private readonly IDbContextFactory<HomePlannerDbContext> _dbFactory;

    public IngredienteRepository(HomePlannerDbContext db, IDbContextFactory<HomePlannerDbContext> dbFactory)
    {
        _db = db;
        _dbFactory = dbFactory;
    }

    public async Task<IReadOnlyList<IngredienteListaDTO>> ListarAsync(
        IngredienteFiltroDTO filtro, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await MontarQueryBase(db, filtro)
            .OrderBy(i => i.Nome)
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            .Select(i => new IngredienteListaDTO
            {
                Id                   = i.Id,
                Nome                 = i.Nome,
                Categoria            = i.Categoria,
                UnidadeMedidaPadraoId = i.UnidadeMedidaPadraoId,
                CodigoUnidadePadrao  = i.UnidadeMedidaPadrao != null ? i.UnidadeMedidaPadrao.Codigo : null,
            })
            .ToListAsync(ct);
    }

    public async Task<int> ContarAsync(IngredienteFiltroDTO filtro, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await MontarQueryBase(db, filtro).CountAsync(ct);
    }

    public async Task<IngredienteDetalheDTO?> ObterDetalheAsync(int id, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.Ingredientes
            .Where(i => i.Id == id)
            .Select(i => new IngredienteDetalheDTO
            {
                Id                   = i.Id,
                Nome                 = i.Nome,
                Categoria            = i.Categoria,
                UnidadeMedidaPadraoId = i.UnidadeMedidaPadraoId,
                CodigoUnidadePadrao  = i.UnidadeMedidaPadrao != null ? i.UnidadeMedidaPadrao.Codigo : null,
                NomeUnidadePadrao    = i.UnidadeMedidaPadrao != null ? i.UnidadeMedidaPadrao.Nome  : null,
                DataCriacao          = i.DataCriacao,
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Ingrediente?> ObterEntidadeAsync(int id, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.Ingredientes.FirstOrDefaultAsync(i => i.Id == id, ct);
    }

    public async Task<IReadOnlyList<IngredienteListaDTO>> BuscarAutoCompleteAsync(
        string textoNormalizado, int limite, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.Ingredientes
            .Where(i => i.NomeNormalizado.Contains(textoNormalizado))
            .OrderBy(i => i.Nome)
            .Take(limite)
            .Select(i => new IngredienteListaDTO
            {
                Id                   = i.Id,
                Nome                 = i.Nome,
                Categoria            = i.Categoria,
                UnidadeMedidaPadraoId = i.UnidadeMedidaPadraoId,
                CodigoUnidadePadrao  = i.UnidadeMedidaPadrao != null ? i.UnidadeMedidaPadrao.Codigo : null,
            })
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<IngredienteListaDTO>> DetectarSimilaresAsync(
        string nomeNormalizado, int? excluirId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var query = db.Ingredientes
            .Where(i => i.NomeNormalizado.Contains(nomeNormalizado)
                     || nomeNormalizado.Contains(i.NomeNormalizado));

        if (excluirId.HasValue)
            query = query.Where(i => i.Id != excluirId.Value);

        return await query
            .Take(5)
            .Select(i => new IngredienteListaDTO
            {
                Id   = i.Id,
                Nome = i.Nome,
            })
            .ToListAsync(ct);
    }

    public async Task<bool> ExisteNomeDuplicadoAsync(
        string nomeNormalizado, int? excluirId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var query = db.Ingredientes.Where(i => i.NomeNormalizado == nomeNormalizado);
        if (excluirId.HasValue) query = query.Where(i => i.Id != excluirId.Value);
        return await query.AnyAsync(ct);
    }

    public async Task AdicionarAsync(Ingrediente entidade, CancellationToken ct = default)
        => await _db.Ingredientes.AddAsync(entidade, ct);

    public Task<int> SalvarAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);

    private static IQueryable<Ingrediente> MontarQueryBase(
        HomePlannerDbContext db, IngredienteFiltroDTO filtro)
    {
        IQueryable<Ingrediente> q = db.Ingredientes;
        if (!string.IsNullOrWhiteSpace(filtro.TextoBusca))
        {
            var busca = filtro.TextoBusca.Trim().ToLowerInvariant();
            q = q.Where(i => i.NomeNormalizado.Contains(busca));
        }
        if (!string.IsNullOrWhiteSpace(filtro.Categoria))
            q = q.Where(i => i.Categoria == filtro.Categoria);
        return q;
    }
}
