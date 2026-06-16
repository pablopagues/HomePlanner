using Application.HomePlanner.Services.Notificacoes;
using Domain.HomePlanner.Models.Enums;
using Infrastructure.HomePlanner.Contexts;
using Microsoft.EntityFrameworkCore;

namespace HomePlanner.BlazorServer.Services;

/// <summary>
/// Serviço de fundo que dispara notificações push de lembrete quando chega a hora de uma tarefa
/// (descontada a antecedência configurada pela família). Roda independente de sessão/usuário logado.
/// </summary>
public class LembreteTarefaBackgroundService : BackgroundService
{
    private static readonly TimeSpan Intervalo = TimeSpan.FromMinutes(1);

    /// <summary>Se o serviço ficou fora do ar, ainda avisa tarefas cuja hora passou há no máximo isto.</summary>
    private static readonly TimeSpan JanelaRecuperacao = TimeSpan.FromHours(2);

    private const string FusoPadrao = "America/Toronto";
    private const int AntecedenciaPadrao = 15;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LembreteTarefaBackgroundService> _logger;

    public LembreteTarefaBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<LembreteTarefaBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Intervalo);
        do
        {
            try
            {
                await ProcessarLembretesAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // desligamento normal
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha no ciclo de lembretes de tarefa.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ProcessarLembretesAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HomePlannerDbContext>();
        var push = scope.ServiceProvider.GetRequiredService<IPushNotificationService>();

        if (!push.Habilitado) return; // sem chaves VAPID, nem varre

        // Limita às tarefas em torno de "hoje" em UTC (cobre qualquer fuso ±). Sem contexto de tenant,
        // por isso IgnoreQueryFilters em tudo.
        var hojeUtc = DateOnly.FromDateTime(DateTime.UtcNow);
        var deUtc = hojeUtc.AddDays(-1);
        var ateUtc = hojeUtc.AddDays(1);

        var candidatas = await db.Tarefas
            .IgnoreQueryFilters()
            .Where(t => !t.IsDeleted
                        && !t.Concluida
                        && t.ResponsavelUsuarioId != null
                        && t.DataPrevista != null
                        && t.HoraInicio != null
                        && t.LembreteEnviadoEm == null
                        && t.DataPrevista >= deUtc
                        && t.DataPrevista <= ateUtc)
            .ToListAsync(ct);

        if (candidatas.Count == 0) return;

        var tenantIds = candidatas.Select(t => t.TenantId).Distinct().ToList();
        var configs = await db.ConfiguracoesFamilia
            .IgnoreQueryFilters()
            .Where(c => tenantIds.Contains(c.TenantId))
            .ToDictionaryAsync(c => c.TenantId, ct);

        var agoraUtc = DateTimeOffset.UtcNow;
        var houveMudanca = false;
        var cachePais = new Dictionary<Guid, List<string>>();
        var cacheNomes = new Dictionary<string, string>();

        foreach (var tarefa in candidatas)
        {
            configs.TryGetValue(tarefa.TenantId, out var config);
            var fuso = config?.FusoHorario ?? FusoPadrao;
            var antecedencia = config?.MinutosAntecedenciaLembrete ?? AntecedenciaPadrao;

            TimeZoneInfo tz;
            try
            {
                tz = TimeZoneInfo.FindSystemTimeZoneById(fuso);
            }
            catch (Exception)
            {
                _logger.LogWarning("Fuso inválido '{Fuso}' (tenant {Tenant}); lembrete pulado.", fuso, tarefa.TenantId);
                continue;
            }

            var agendadoLocal = tarefa.DataPrevista!.Value.ToDateTime(tarefa.HoraInicio!.Value);
            var agoraLocal = TimeZoneInfo.ConvertTime(agoraUtc, tz).DateTime;
            var disparoLocal = agendadoLocal.AddMinutes(-antecedencia);

            if (agoraLocal < disparoLocal)
                continue; // ainda não é hora — reavalia no próximo ciclo

            // Velho demais (passou da janela de recuperação): marca como processado e não envia.
            if (agoraLocal > agendadoLocal.Add(JanelaRecuperacao))
            {
                tarefa.LembreteEnviadoEm = agoraUtc.UtcDateTime;
                houveMudanca = true;
                continue;
            }

            // Texto resolvido no idioma do destinatário dentro do serviço de push.
            await push.EnviarLembreteTarefaAsync(
                tarefa.TenantId, tarefa.ResponsavelUsuarioId!, tarefa.Titulo, tarefa.HoraInicio.Value, tarefa.Id, ct);

            // "Notificar pai/mãe": também avisa os Owner/Membro da família (sem duplicar o responsável).
            if (tarefa.NotificarResponsaveis)
            {
                var paisIds = await ObterPaisAsync(db, tarefa.TenantId, cachePais, ct);
                var nomeResponsavel = await ObterNomeAsync(db, tarefa.ResponsavelUsuarioId!, cacheNomes, ct);
                foreach (var paiId in paisIds.Where(p => p != tarefa.ResponsavelUsuarioId))
                {
                    await push.EnviarLembreteTarefaPaisAsync(
                        tarefa.TenantId, paiId, nomeResponsavel, tarefa.Titulo, tarefa.HoraInicio.Value, tarefa.Id, ct);
                }
            }

            tarefa.LembreteEnviadoEm = agoraUtc.UtcDateTime;
            houveMudanca = true;
        }

        if (houveMudanca)
            await db.SaveChangesAsync(ct);
    }

    /// <summary>IDs dos pais (Owner/Membro) de um tenant, com cache por ciclo.</summary>
    private static async Task<List<string>> ObterPaisAsync(
        HomePlannerDbContext db, Guid tenantId, Dictionary<Guid, List<string>> cache, CancellationToken ct)
    {
        if (cache.TryGetValue(tenantId, out var existente))
            return existente;

        // Papel é ITenantEntity (filtro global por tenant) → IgnoreQueryFilters fora de requisição.
        var papeisIds = await db.Roles
            .IgnoreQueryFilters()
            .Where(p => p.TenantId == tenantId && (p.Tipo == PapelUsuario.Owner || p.Tipo == PapelUsuario.Membro))
            .Select(p => p.Id)
            .ToListAsync(ct);

        var paisIds = await db.UserRoles
            .Where(ur => papeisIds.Contains(ur.RoleId))
            .Select(ur => ur.UserId)
            .Distinct()
            .ToListAsync(ct);

        cache[tenantId] = paisIds;
        return paisIds;
    }

    /// <summary>Nome do usuário, com cache por ciclo.</summary>
    private static async Task<string> ObterNomeAsync(
        HomePlannerDbContext db, string usuarioId, Dictionary<string, string> cache, CancellationToken ct)
    {
        if (cache.TryGetValue(usuarioId, out var existente))
            return existente;

        var nome = await db.Users
            .IgnoreQueryFilters()
            .Where(u => u.Id == usuarioId)
            .Select(u => u.NomeCompleto)
            .FirstOrDefaultAsync(ct) ?? string.Empty;

        cache[usuarioId] = nome;
        return nome;
    }
}
