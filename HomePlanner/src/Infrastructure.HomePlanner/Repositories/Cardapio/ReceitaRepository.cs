using Application.HomePlanner.DTOs.Cardapio.Receita;
using Application.HomePlanner.Repositories.Cardapio;
using Domain.HomePlanner.Models.Cardapio;
using Infrastructure.HomePlanner.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.HomePlanner.Repositories.Cardapio;

public class ReceitaRepository : IReceitaRepository
{
    private readonly HomePlannerDbContext _db;
    private readonly IDbContextFactory<HomePlannerDbContext> _dbFactory;

    public ReceitaRepository(HomePlannerDbContext db, IDbContextFactory<HomePlannerDbContext> dbFactory)
    {
        _db = db;
        _dbFactory = dbFactory;
    }

    public async Task<IReadOnlyList<ReceitaListaDTO>> ListarAsync(
        ReceitaFiltroDTO filtro, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await MontarQueryBase(db, filtro)
            .OrderBy(r => r.Nome)
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            .Select(r => new ReceitaListaDTO
            {
                Id                = r.Id,
                Nome              = r.Nome,
                NumeroPorcoesBase = r.NumeroPorcoesBase,
                TempoPreparoMinutos = r.TempoPreparoMinutos,
                UrlImagem         = r.UrlImagem,
                TotalIngredientes = r.Ingredientes.Count(ri => !ri.IsDeleted),
                DataCriacao       = r.DataCriacao,
            })
            .ToListAsync(ct);
    }

    public async Task<int> ContarAsync(ReceitaFiltroDTO filtro, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await MontarQueryBase(db, filtro).CountAsync(ct);
    }

    public async Task<ReceitaDetalheDTO?> ObterDetalheAsync(int id, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.Receitas
            .Where(r => r.Id == id)
            .Select(r => new ReceitaDetalheDTO
            {
                Id                = r.Id,
                Nome              = r.Nome,
                ModoPreparo       = r.ModoPreparo,
                NumeroPorcoesBase = r.NumeroPorcoesBase,
                TempoPreparoMinutos = r.TempoPreparoMinutos,
                UrlOrigem         = r.UrlOrigem,
                UrlImagem         = r.UrlImagem,
                Observacoes       = r.Observacoes,
                DataCriacao       = r.DataCriacao,
                Ingredientes      = r.Ingredientes
                    .Where(ri => !ri.IsDeleted)
                    .OrderBy(ri => ri.Ordem)
                    .Select(ri => new ReceitaIngredienteDTO
                    {
                        Id             = ri.Id,
                        IngredienteId  = ri.IngredienteId,
                        NomeIngrediente = ri.Ingrediente.Nome,
                        Quantidade     = ri.Quantidade,
                        UnidadeMedidaId = ri.UnidadeMedidaId,
                        CodigoUnidade  = ri.UnidadeMedida.Codigo,
                        NomeUnidade    = ri.UnidadeMedida.Nome,
                        Observacao     = ri.Observacao,
                        Opcional       = ri.Opcional,
                        Ordem          = ri.Ordem,
                    })
                    .ToList(),
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Receita?> ObterEntidadeComIngredientesAsync(int id, CancellationToken ct = default)
    {
        // Usa _db (contexto rastreado) — a entidade será mutada e persistida via SalvarAsync().
        return await _db.Receitas
            .Include(r => r.Ingredientes.Where(ri => !ri.IsDeleted))
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task AdicionarAsync(Receita entidade, CancellationToken ct = default)
        => await _db.Receitas.AddAsync(entidade, ct);

    public Task<int> SalvarAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);

    private static IQueryable<Receita> MontarQueryBase(HomePlannerDbContext db, ReceitaFiltroDTO filtro)
    {
        IQueryable<Receita> q = db.Receitas;
        if (!string.IsNullOrWhiteSpace(filtro.TextoBusca))
        {
            var busca = filtro.TextoBusca.Trim().ToLowerInvariant();
            q = q.Where(r => r.NomeNormalizado.Contains(busca));
        }
        return q;
    }
}
