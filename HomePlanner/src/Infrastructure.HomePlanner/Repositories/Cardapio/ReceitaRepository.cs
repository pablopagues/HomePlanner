using Application.HomePlanner.DTOs.Cardapio.Receita;
using Application.HomePlanner.Repositories.Cardapio;
using Application.HomePlanner.Services.Cardapio;
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
                TemFoto           = r.Foto != null,   // IS NOT NULL — não transfere o blob
                FotoAtualizadaEm  = r.FotoAtualizadaEm,
                TotalIngredientes = r.Ingredientes.Count(ri => !ri.IsDeleted),
                TotalComponentes  = r.Componentes.Count(rc => !rc.IsDeleted),
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
                TemFoto           = r.Foto != null,   // IS NOT NULL — não transfere o blob
                FotoAtualizadaEm  = r.FotoAtualizadaEm,
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
                Componentes       = r.Componentes
                    .Where(rc => !rc.IsDeleted)
                    .OrderBy(rc => rc.Ordem)
                    .Select(rc => new ReceitaComponenteDetalheDTO
                    {
                        Id               = rc.Id,
                        ComponenteId     = rc.ReceitaComponenteId,
                        Nome             = rc.Componente.Nome,
                        PorcoesDesejadas = rc.PorcoesDesejadas,
                        ModoPreparo      = rc.Componente.ModoPreparo,
                        Ordem            = rc.Ordem,
                    })
                    .ToList(),
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<GrafoReceitas> ObterGrafoReceitasAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        // db.Receitas já vem filtrado por tenant + não-deletado (query filter global).
        var porcoes = await db.Receitas
            .Select(r => new { r.Id, r.NumeroPorcoesBase })
            .ToListAsync(ct);

        var ingredientes = await db.Receitas
            .SelectMany(r => r.Ingredientes
                .Where(ri => !ri.IsDeleted)
                .Select(ri => new
                {
                    ReceitaId = r.Id,
                    ri.IngredienteId,
                    Nome        = ri.Ingrediente.Nome,
                    ri.Quantidade,
                    ri.UnidadeMedidaId,
                    Codigo      = ri.UnidadeMedida.Codigo,
                    NomeUnidade = ri.UnidadeMedida.Nome,
                }))
            .ToListAsync(ct);

        var componentes = await db.Receitas
            .SelectMany(r => r.Componentes
                .Where(rc => !rc.IsDeleted)
                .Select(rc => new { ReceitaId = r.Id, rc.ReceitaComponenteId, rc.PorcoesDesejadas }))
            .ToListAsync(ct);

        return new GrafoReceitas
        {
            PorcoesBase = porcoes.ToDictionary(p => p.Id, p => p.NumeroPorcoesBase),
            Ingredientes = ingredientes
                .GroupBy(i => i.ReceitaId)
                .ToDictionary(
                    g => g.Key,
                    g => (IReadOnlyList<IngredienteExpandidoDTO>)g.Select(i => new IngredienteExpandidoDTO
                    {
                        IngredienteId   = i.IngredienteId,
                        NomeIngrediente = i.Nome,
                        Quantidade      = i.Quantidade,
                        UnidadeMedidaId = i.UnidadeMedidaId,
                        CodigoUnidade   = i.Codigo,
                        NomeUnidade     = i.NomeUnidade,
                    }).ToList()),
            Componentes = componentes
                .GroupBy(c => c.ReceitaId)
                .ToDictionary(
                    g => g.Key,
                    g => (IReadOnlyList<ComponenteAresta>)g
                        .Select(c => new ComponenteAresta(c.ReceitaComponenteId, c.PorcoesDesejadas))
                        .ToList()),
        };
    }

    public async Task<IReadOnlyList<string>> ObterPaisQueUsamAsync(
        int componenteId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.Receitas
            .Where(r => r.Componentes.Any(rc => !rc.IsDeleted && rc.ReceitaComponenteId == componenteId))
            .OrderBy(r => r.Nome)
            .Select(r => r.Nome)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ReceitaListaDTO>> BuscarAutoCompleteAsync(
        string textoNormalizado, int limite, int? excluirId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var query = db.Receitas.AsQueryable();
        if (!string.IsNullOrWhiteSpace(textoNormalizado))
            query = query.Where(r => r.NomeNormalizado.Contains(textoNormalizado));
        if (excluirId.HasValue)
            query = query.Where(r => r.Id != excluirId.Value);

        return await query
            .OrderBy(r => r.Nome)
            .Take(limite)
            .Select(r => new ReceitaListaDTO
            {
                Id                = r.Id,
                Nome              = r.Nome,
                NumeroPorcoesBase = r.NumeroPorcoesBase,
                TempoPreparoMinutos = r.TempoPreparoMinutos,
                UrlImagem         = r.UrlImagem,
                TemFoto           = r.Foto != null,   // IS NOT NULL — não transfere o blob
                FotoAtualizadaEm  = r.FotoAtualizadaEm,
                TotalIngredientes = r.Ingredientes.Count(ri => !ri.IsDeleted),
                TotalComponentes  = r.Componentes.Count(rc => !rc.IsDeleted),
                DataCriacao       = r.DataCriacao,
            })
            .ToListAsync(ct);
    }

    public async Task<ReceitaFotoDTO?> ObterFotoAsync(int id, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        // db.Receitas já vem filtrado por tenant + não-deletado (query filter global):
        // só serve a foto de receitas da própria família.
        var info = await db.Receitas
            .Where(r => r.Id == id)
            .Select(r => new { r.Foto, r.FotoContentType, r.FotoAtualizadaEm })
            .FirstOrDefaultAsync(ct);

        if (info?.Foto is null || info.Foto.Length == 0)
            return null;

        return new ReceitaFotoDTO
        {
            Conteudo = info.Foto,
            ContentType = string.IsNullOrWhiteSpace(info.FotoContentType)
                ? "application/octet-stream"
                : info.FotoContentType,
            Versao = (info.FotoAtualizadaEm ?? DateTime.UnixEpoch).Ticks.ToString(),
        };
    }

    public async Task<Receita?> ObterEntidadeComIngredientesAsync(int id, CancellationToken ct = default)
    {
        // Usa _db (contexto rastreado) — a entidade será mutada e persistida via SalvarAsync().
        return await _db.Receitas
            .Include(r => r.Ingredientes.Where(ri => !ri.IsDeleted))
            .Include(r => r.Componentes.Where(rc => !rc.IsDeleted))
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
