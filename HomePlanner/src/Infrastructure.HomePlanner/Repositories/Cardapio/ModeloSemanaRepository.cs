using Application.HomePlanner.DTOs.Cardapio.ModeloSemana;
using Application.HomePlanner.Repositories.Cardapio;
using Domain.HomePlanner.Models.Cardapio;
using Infrastructure.HomePlanner.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.HomePlanner.Repositories.Cardapio;

public class ModeloSemanaRepository : IModeloSemanaRepository
{
    private readonly HomePlannerDbContext _db;
    private readonly IDbContextFactory<HomePlannerDbContext> _dbFactory;

    public ModeloSemanaRepository(HomePlannerDbContext db, IDbContextFactory<HomePlannerDbContext> dbFactory)
    {
        _db = db;
        _dbFactory = dbFactory;
    }

    public async Task<IReadOnlyList<ModeloSemanaListaDTO>> ListarAsync(
        ModeloSemanaFiltroDTO filtro, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await MontarQueryBase(db, filtro)
            .OrderBy(m => m.Nome)
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            .Select(m => new ModeloSemanaListaDTO
            {
                Id             = m.Id,
                Nome           = m.Nome,
                Descricao      = m.Descricao,
                TotalRefeicoes = m.RefeicoesModelo.Count(r => !r.IsDeleted),
            })
            .ToListAsync(ct);
    }

    public async Task<int> ContarAsync(ModeloSemanaFiltroDTO filtro, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await MontarQueryBase(db, filtro).CountAsync(ct);
    }

    public async Task<ModeloSemanaDetalheDTO?> ObterDetalheAsync(int id, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.ModelosSemana
            .Where(m => m.Id == id)
            .Select(m => new ModeloSemanaDetalheDTO
            {
                Id        = m.Id,
                Nome      = m.Nome,
                Descricao = m.Descricao,
                Refeicoes = m.RefeicoesModelo
                    .Where(r => !r.IsDeleted)
                    .OrderBy(r => r.DiaSemana).ThenBy(r => r.TipoRefeicao)
                    .Select(r => new ModeloSemanaRefeicaoDTO
                    {
                        Id               = r.Id,
                        DiaSemana        = r.DiaSemana,
                        TipoRefeicao     = r.TipoRefeicao,
                        ReceitaId        = r.ReceitaId,
                        ReceitaNome      = r.Receita != null ? r.Receita.Nome : null,
                        PorcoesDesejadas = r.PorcoesDesejadas,
                        Observacao       = r.Observacao,
                    })
                    .ToList(),
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<ModeloSemana?> ObterEntidadeComRefeicoesAsync(int id, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.ModelosSemana
            .Include(m => m.RefeicoesModelo.Where(r => !r.IsDeleted))
            .FirstOrDefaultAsync(m => m.Id == id, ct);
    }

    public async Task AdicionarAsync(ModeloSemana entidade, CancellationToken ct = default)
        => await _db.ModelosSemana.AddAsync(entidade, ct);

    public Task<int> SalvarAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);

    private static IQueryable<ModeloSemana> MontarQueryBase(HomePlannerDbContext db, ModeloSemanaFiltroDTO filtro)
    {
        IQueryable<ModeloSemana> q = db.ModelosSemana;
        if (!string.IsNullOrWhiteSpace(filtro.TextoBusca))
        {
            var busca = filtro.TextoBusca.Trim().ToLowerInvariant();
            q = q.Where(m => m.Nome.ToLower().Contains(busca));
        }
        return q;
    }
}
