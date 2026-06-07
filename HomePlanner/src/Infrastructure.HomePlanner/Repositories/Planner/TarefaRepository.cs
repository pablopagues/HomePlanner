using Application.HomePlanner.DTOs.Planner;
using Application.HomePlanner.Repositories.Planner;
using Domain.HomePlanner.Models.Enums;
using Domain.HomePlanner.Models.Planner;
using Infrastructure.HomePlanner.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.HomePlanner.Repositories.Planner;

public class TarefaRepository : ITarefaRepository
{
    private readonly HomePlannerDbContext _db;
    private readonly IDbContextFactory<HomePlannerDbContext> _dbFactory;

    public TarefaRepository(HomePlannerDbContext db, IDbContextFactory<HomePlannerDbContext> dbFactory)
    {
        _db = db;
        _dbFactory = dbFactory;
    }

    public async Task<IReadOnlyList<TarefaListaDTO>> ListarAsync(
        TarefaFiltroDTO filtro, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await MontarQueryBase(db, filtro)
            .OrderBy(t => t.Concluida)
            .ThenBy(t => t.DataPrevista ?? DateOnly.MaxValue)
            .ThenBy(t => t.HoraInicio ?? TimeOnly.MaxValue)
            .ThenBy(t => t.Titulo)
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            .Select(t => new TarefaListaDTO
            {
                Id                   = t.Id,
                Titulo               = t.Titulo,
                Descricao            = t.Descricao,
                DataPrevista         = t.DataPrevista,
                HoraInicio           = t.HoraInicio,
                HoraFim              = t.HoraFim,
                Concluida            = t.Concluida,
                Recorrencia          = t.Recorrencia,
                Visibilidade         = t.Visibilidade,
                ResponsavelUsuarioId = t.ResponsavelUsuarioId,
                ResponsavelNome      = t.Responsavel != null ? t.Responsavel.NomeCompleto : null,
                CriadoPorUsuarioId   = t.CriadoPorUsuarioId,
                ResponsavelFotoAtualizadaEm = t.Responsavel != null ? t.Responsavel.FotoAtualizadaEm : null,
            })
            .ToListAsync(ct);
    }

    public async Task<int> ContarAsync(TarefaFiltroDTO filtro, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await MontarQueryBase(db, filtro).CountAsync(ct);
    }

    public async Task<Tarefa?> ObterEntidadeAsync(int id, CancellationToken ct = default)
        => await _db.Tarefas.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<IReadOnlyList<MembroFamiliaDTO>> ListarMembrosFamiliaAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        // Filtro global de tenant já aplicado a Usuarios
        return await db.Users
            .Where(u => u.Ativo)
            .OrderBy(u => u.NomeCompleto)
            .Select(u => new MembroFamiliaDTO
            {
                UsuarioId        = u.Id,
                Nome             = u.NomeCompleto,
                FotoAtualizadaEm = u.FotoAtualizadaEm,
            })
            .ToListAsync(ct);
    }

    public async Task AdicionarAsync(Tarefa entidade, CancellationToken ct = default)
        => await _db.Tarefas.AddAsync(entidade, ct);

    public Task<int> SalvarAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);

    private static IQueryable<Tarefa> MontarQueryBase(HomePlannerDbContext db, TarefaFiltroDTO filtro)
    {
        IQueryable<Tarefa> q = db.Tarefas;

        if (!string.IsNullOrWhiteSpace(filtro.TextoBusca))
        {
            var busca = filtro.TextoBusca.Trim().ToLower();
            q = q.Where(t => t.Titulo.ToLower().Contains(busca));
        }
        if (filtro.Concluida.HasValue)
            q = q.Where(t => t.Concluida == filtro.Concluida.Value);
        if (!string.IsNullOrWhiteSpace(filtro.ResponsavelUsuarioId))
            q = q.Where(t => t.ResponsavelUsuarioId == filtro.ResponsavelUsuarioId);
        // Visibilidade: tarefas Privadas só aparecem para quem as criou.
        if (!string.IsNullOrWhiteSpace(filtro.UsuarioAtualId))
            q = q.Where(t => t.Visibilidade == VisibilidadeTarefa.Familia
                          || t.CriadoPorUsuarioId == filtro.UsuarioAtualId);
        if (filtro.DataDe.HasValue)
            q = q.Where(t => t.DataPrevista != null && t.DataPrevista >= filtro.DataDe.Value);
        if (filtro.DataAte.HasValue)
            q = q.Where(t => t.DataPrevista != null && t.DataPrevista <= filtro.DataAte.Value);

        return q;
    }
}
