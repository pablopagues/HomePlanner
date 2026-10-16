using Application.HomePlanner.DTOs.Cardapio.CardapioIA;
using Application.HomePlanner.Middleware;
using Application.HomePlanner.Services.Cardapio;
using Application.HomePlanner.Services.Notificacoes;
using Domain.HomePlanner.Models.Enums;
using Domain.HomePlanner.Models.SaaS.Options;
using Infrastructure.HomePlanner.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HomePlanner.BlazorServer.Services;

/// <summary>
/// Rascunho automático do cardápio por IA: no dia/hora que a família agendou (no fuso dela),
/// prepara o cardápio da semana seguinte e avisa os adultos por push. Se o serviço ficou fora
/// do ar no horário, gera assim que voltar, até o domingo — o rascunho é sempre da próxima segunda.
/// </summary>
public class CardapioAutomaticoBackgroundService : BackgroundService
{
    private static readonly TimeSpan Intervalo = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan AtrasoInicial = TimeSpan.FromMinutes(1);
    private const string FusoPadrao = "America/Toronto";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CardapioAutomaticoBackgroundService> _logger;

    public CardapioAutomaticoBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<CardapioAutomaticoBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(AtrasoInicial, stoppingToken); // deixa a aplicação terminar de subir
        }
        catch (OperationCanceledException)
        {
            return;
        }

        using var timer = new PeriodicTimer(Intervalo);
        do
        {
            try
            {
                await ProcessarAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // desligamento normal
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha no ciclo do cardápio automático.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ProcessarAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var gerador = scope.ServiceProvider.GetRequiredService<IGeradorCardapioIA>();
        if (!gerador.Habilitado) return; // sem chave da Anthropic, nem varre

        var db = scope.ServiceProvider.GetRequiredService<HomePlannerDbContext>();

        // Sem contexto de tenant aqui: IgnoreQueryFilters em tudo.
        var agendamentos = await db.AgendamentosCardapioIA
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(a => a.Ativo)
            .ToListAsync(ct);
        if (agendamentos.Count == 0) return;

        var tenantIds = agendamentos.Select(a => a.TenantId).ToList();
        var fusos = await db.ConfiguracoesFamilia
            .IgnoreQueryFilters()
            .Where(c => tenantIds.Contains(c.TenantId))
            .ToDictionaryAsync(c => c.TenantId, c => c.FusoHorario, ct);

        var agoraUtc = DateTimeOffset.UtcNow;
        foreach (var agendamento in agendamentos)
        {
            var fuso = fusos.GetValueOrDefault(agendamento.TenantId) ?? FusoPadrao;
            TimeZoneInfo tz;
            try
            {
                tz = TimeZoneInfo.FindSystemTimeZoneById(fuso);
            }
            catch (Exception)
            {
                _logger.LogWarning("Fuso inválido '{Fuso}' (tenant {Tenant}); cardápio automático pulado.", fuso, agendamento.TenantId);
                continue;
            }

            var agoraLocal = TimeZoneInfo.ConvertTime(agoraUtc, tz).DateTime;
            var hoje = DateOnly.FromDateTime(agoraLocal);
            var diaHoje = hoje.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)hoje.DayOfWeek; // 1=segunda … 7=domingo

            // Alvo: a próxima segunda. Disparo: o dia/hora agendado nesta semana (se já passou
            // nesta semana, ainda vale — recupera um ciclo perdido até o domingo).
            var alvo = hoje.AddDays(8 - diaHoje);
            var disparo = hoje.AddDays((int)agendamento.DiaSemana - diaHoje)
                .ToDateTime(new TimeOnly(Math.Clamp(agendamento.Hora, 0, 23), 0));
            if (agoraLocal < disparo) continue;

            // Atalho barato: rascunho da semana já resolvido → nada a fazer.
            var status = await db.RascunhosCardapioIA
                .IgnoreQueryFilters()
                .Where(r => r.TenantId == agendamento.TenantId && r.DataInicio == alvo)
                .Select(r => (StatusRascunhoCardapio?)r.Status)
                .FirstOrDefaultAsync(ct);
            if (status is not null and not StatusRascunhoCardapio.Gerando) continue;

            try
            {
                await ProcessarTenantAsync(agendamento.TenantId, alvo, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Falha no cardápio automático do tenant {Tenant} ({Data}).", agendamento.TenantId, alvo);
            }
        }
    }

    // Um escopo por família, com o contexto de tenant definido como o Owner: os serviços
    // normais (filtros de tenant, cota, permissão de adulto) funcionam como numa requisição.
    private async Task ProcessarTenantAsync(Guid tenantId, DateOnly alvo, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var sp = scope.ServiceProvider;

        var assinatura = await sp.GetRequiredService<IAssinaturaStatusReader>().ObterAsync(tenantId, ct);
        if (assinatura.Bloqueada) return;

        var db = sp.GetRequiredService<HomePlannerDbContext>();
        var adultos = await ObterAdultosAsync(db, tenantId, ct);
        var owner = adultos.FirstOrDefault(a => a.Owner) ?? adultos.FirstOrDefault();
        if (owner is null) return;

        sp.GetRequiredService<TenantContext>().Definir(tenantId, owner.Id, owner.Nome, restritoAsProprias: false, ehOwner: owner.Owner);

        var idiomaPadrao = sp.GetRequiredService<IOptions<LocalizacaoOptions>>().Value.IdiomaPadrao;
        // Idioma dos motivos/observações da IA: o do Owner; senão o padrão do app ("pt-BR" → "pt").
        var idioma = PublicLanguageService.IsLangValida(owner.Idioma)
            ? owner.Idioma!
            : idiomaPadrao.Split('-')[0].ToLowerInvariant();

        var resultado = await sp.GetRequiredService<ICardapioAutomaticoService>().GerarRascunhoAsync(alvo, idioma, ct);
        _logger.LogInformation("Cardápio automático do tenant {Tenant} para {Data}: {Resultado}.", tenantId, alvo, resultado);

        if (resultado != ResultadoRascunhoAutomatico.Pronto) return;

        var push = sp.GetRequiredService<IPushNotificationService>();
        foreach (var adulto in adultos)
        {
            try
            {
                await push.EnviarCardapioProntoAsync(tenantId, adulto.Id, alvo, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Falha ao avisar cardápio pronto (usuario={UsuarioId}).", adulto.Id);
            }
        }
    }

    private sealed record Adulto(string Id, string Nome, string? Idioma, bool Owner);

    /// <summary>Owner e Membro (pai/mãe) ativos da família.</summary>
    private static async Task<List<Adulto>> ObterAdultosAsync(HomePlannerDbContext db, Guid tenantId, CancellationToken ct)
    {
        // Papel é ITenantEntity (filtro global por tenant) → IgnoreQueryFilters fora de requisição.
        var papeis = await db.Roles
            .IgnoreQueryFilters()
            .Where(p => p.TenantId == tenantId && (p.Tipo == PapelUsuario.Owner || p.Tipo == PapelUsuario.Membro))
            .Select(p => new { p.Id, p.Tipo })
            .ToListAsync(ct);
        var papelOwner = papeis.Where(p => p.Tipo == PapelUsuario.Owner).Select(p => p.Id).ToHashSet();
        var papelIds = papeis.Select(p => p.Id).ToList();

        var vinculos = await db.UserRoles
            .Where(ur => papelIds.Contains(ur.RoleId))
            .Select(ur => new { ur.UserId, ur.RoleId })
            .ToListAsync(ct);
        var ids = vinculos.Select(v => v.UserId).Distinct().ToList();

        var usuarios = await db.Users
            .IgnoreQueryFilters()
            .Where(u => ids.Contains(u.Id) && u.Ativo && !u.IsDeleted)
            .Select(u => new { u.Id, u.NomeCompleto, u.Idioma })
            .ToListAsync(ct);

        return usuarios
            .Select(u => new Adulto(u.Id, u.NomeCompleto, u.Idioma,
                vinculos.Any(v => v.UserId == u.Id && papelOwner.Contains(v.RoleId))))
            .ToList();
    }
}
