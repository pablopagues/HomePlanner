using System.Net;
using System.Text.Json;
using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Notificacoes;
using Application.HomePlanner.Middleware;
using Application.HomePlanner.Services.Notificacoes;
using Domain.HomePlanner.Models.Notificacoes;
using Domain.HomePlanner.Models.SaaS.Options;
using Infrastructure.HomePlanner.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebPush;

namespace Infrastructure.HomePlanner.Services.Notificacoes;

public class PushNotificationService : IPushNotificationService
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HomePlannerDbContext _db;
    private readonly TenantContext _tenantContext;
    private readonly TenantContextAccessor _tenantAccessor;
    private readonly INotificacaoTextoService _texto;
    private readonly WebPushOptions _options;
    private readonly ILogger<PushNotificationService> _logger;
    private readonly WebPushClient _client = new();

    public PushNotificationService(
        HomePlannerDbContext db,
        TenantContext tenantContext,
        TenantContextAccessor tenantAccessor,
        INotificacaoTextoService texto,
        IOptions<WebPushOptions> options,
        ILogger<PushNotificationService> logger)
    {
        _db = db;
        _tenantContext = tenantContext;
        _tenantAccessor = tenantAccessor;
        _texto = texto;
        _options = options.Value;
        _logger = logger;
    }

    public bool Habilitado => _options.EstaConfigurado;

    public string ChavePublica => _options.EstaConfigurado ? _options.PublicKey : string.Empty;

    public async Task<ResultadoOperacao> RegistrarInscricaoAsync(InscricaoPushDTO dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Endpoint) || string.IsNullOrWhiteSpace(dto.P256dh) || string.IsNullOrWhiteSpace(dto.Auth))
            return ResultadoOperacao.Falha("Dados de inscrição incompletos.");

        await _tenantAccessor.GarantirHidratadoAsync();
        var tenantId = _tenantContext.TenantId ?? Guid.Empty;
        var usuarioId = _tenantContext.UsuarioId;
        if (usuarioId is null) return ResultadoOperacao.Falha("Usuário não autenticado.");

        // Upsert por endpoint — inclui registros soft-deleted para reaproveitar (re-ativar).
        var existente = await _db.InscricoesPush
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(i => i.Endpoint == dto.Endpoint, ct);

        if (existente is null)
        {
            _db.InscricoesPush.Add(new InscricaoPush
            {
                TenantId = tenantId,
                UsuarioId = usuarioId,
                Endpoint = dto.Endpoint,
                P256dh = dto.P256dh,
                Auth = dto.Auth,
                UserAgent = Truncar(dto.UserAgent, 512),
                DataCriacao = DateTime.UtcNow,
            });
        }
        else
        {
            existente.TenantId = tenantId;
            existente.UsuarioId = usuarioId;
            existente.P256dh = dto.P256dh;
            existente.Auth = dto.Auth;
            existente.UserAgent = Truncar(dto.UserAgent, 512);
            existente.IsDeleted = false;
            existente.DeletedAt = null;
            existente.DeletedByUsuarioId = null;
        }

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Inscrição de push registrada (usuario={UsuarioId}).", usuarioId);
        return ResultadoOperacao.Ok();
    }

    public async Task RemoverInscricaoAsync(string endpoint, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(endpoint)) return;

        var inscricao = await _db.InscricoesPush.FirstOrDefaultAsync(i => i.Endpoint == endpoint, ct);
        if (inscricao is null) return;

        inscricao.IsDeleted = true;
        inscricao.DeletedAt = DateTime.UtcNow;
        inscricao.DeletedByUsuarioId = _tenantContext.UsuarioId;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> UsuarioTemInscricaoAsync(CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        var usuarioId = _tenantContext.UsuarioId;
        if (usuarioId is null) return false;

        return await _db.InscricoesPush.AnyAsync(i => i.UsuarioId == usuarioId, ct);
    }

    public async Task<int> EnviarTesteAsync(string titulo, string corpo, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        var tenantId = _tenantContext.TenantId ?? Guid.Empty;
        var usuarioId = _tenantContext.UsuarioId;
        if (usuarioId is null) return 0;

        return await EnviarParaUsuarioAsync(tenantId, usuarioId, new NotificacaoPushDTO
        {
            Titulo = titulo,
            Corpo = corpo,
            Url = "/perfil",
            Tag = "teste",
        }, ct);
    }

    public async Task<int> EnviarLembreteTarefaAsync(
        Guid tenantId, string usuarioId, string tituloTarefa, TimeOnly hora, int tarefaId, CancellationToken ct = default)
    {
        var idioma = await ObterIdiomaAsync(usuarioId, ct);
        var (titulo, corpo) = _texto.LembreteTarefa(idioma, tituloTarefa, hora);
        return await EnviarParaUsuarioAsync(tenantId, usuarioId, new NotificacaoPushDTO
        {
            Titulo = titulo, Corpo = corpo, Url = "/calendario", Tag = $"lembrete-{tarefaId}",
        }, ct);
    }

    public async Task<int> EnviarLembreteTarefaPaisAsync(
        Guid tenantId, string paiUsuarioId, string nomeResponsavel, string tituloTarefa, TimeOnly hora, int tarefaId, CancellationToken ct = default)
    {
        var idioma = await ObterIdiomaAsync(paiUsuarioId, ct);
        var (titulo, corpo) = _texto.LembreteTarefaPais(idioma, tituloTarefa, nomeResponsavel, hora);
        return await EnviarParaUsuarioAsync(tenantId, paiUsuarioId, new NotificacaoPushDTO
        {
            Titulo = titulo, Corpo = corpo, Url = "/calendario", Tag = $"lembrete-{tarefaId}-pais",
        }, ct);
    }

    public async Task<int> EnviarTarefaAtribuidaAsync(
        Guid tenantId, string usuarioId, string tituloTarefa, int tarefaId, CancellationToken ct = default)
    {
        var idioma = await ObterIdiomaAsync(usuarioId, ct);
        var (titulo, corpo) = _texto.TarefaAtribuida(idioma, tituloTarefa);
        return await EnviarParaUsuarioAsync(tenantId, usuarioId, new NotificacaoPushDTO
        {
            Titulo = titulo, Corpo = corpo, Url = "/planner", Tag = $"tarefa-{tarefaId}",
        }, ct);
    }

    private async Task<string?> ObterIdiomaAsync(string usuarioId, CancellationToken ct)
        => await _db.Users.IgnoreQueryFilters()
            .Where(u => u.Id == usuarioId)
            .Select(u => u.Idioma)
            .FirstOrDefaultAsync(ct);

    public async Task<int> EnviarParaUsuarioAsync(
        Guid tenantId, string usuarioId, NotificacaoPushDTO notificacao, CancellationToken ct = default)
    {
        if (!_options.EstaConfigurado)
        {
            _logger.LogWarning("Push desabilitado (chaves VAPID ausentes) — notificação não enviada.");
            return 0;
        }

        // IgnoreQueryFilters: pode rodar fora do contexto de requisição (background), sem tenant atual.
        var inscricoes = await _db.InscricoesPush
            .IgnoreQueryFilters()
            .Where(i => i.TenantId == tenantId && i.UsuarioId == usuarioId && !i.IsDeleted)
            .ToListAsync(ct);

        if (inscricoes.Count == 0) return 0;

        var vapid = new VapidDetails(_options.Subject, _options.PublicKey, _options.PrivateKey);
        var payload = JsonSerializer.Serialize(new
        {
            titulo = notificacao.Titulo,
            corpo = notificacao.Corpo,
            url = notificacao.Url ?? "/",
            icone = notificacao.Icone ?? "/favicon.png",
            tag = notificacao.Tag,
        }, JsonOpts);

        var enviados = 0;
        foreach (var inscricao in inscricoes)
        {
            ct.ThrowIfCancellationRequested();
            var pushSub = new WebPush.PushSubscription(inscricao.Endpoint, inscricao.P256dh, inscricao.Auth);
            try
            {
                await _client.SendNotificationAsync(pushSub, payload, vapid);
                inscricao.UltimoEnvioEm = DateTime.UtcNow;
                enviados++;
            }
            catch (WebPushException ex) when (ex.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Gone)
            {
                // Assinatura expirada/cancelada no navegador — remove para não tentar de novo.
                inscricao.IsDeleted = true;
                inscricao.DeletedAt = DateTime.UtcNow;
                _logger.LogInformation("Inscrição de push expirada removida (usuario={UsuarioId}).", usuarioId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao enviar push (usuario={UsuarioId}).", usuarioId);
            }
        }

        await _db.SaveChangesAsync(ct);
        return enviados;
    }

    private static string? Truncar(string? valor, int max)
        => string.IsNullOrEmpty(valor) || valor.Length <= max ? valor : valor[..max];
}
