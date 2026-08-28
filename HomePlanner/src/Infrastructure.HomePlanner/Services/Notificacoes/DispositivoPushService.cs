using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Notificacoes;
using Application.HomePlanner.Middleware;
using Application.HomePlanner.Services.Notificacoes;
using Domain.HomePlanner.Models.Notificacoes;
using Domain.HomePlanner.Models.SaaS.Options;
using FirebaseAdmin.Messaging;
using Infrastructure.HomePlanner.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FcmOptions = Domain.HomePlanner.Models.SaaS.Options.FcmOptions;

namespace Infrastructure.HomePlanner.Services.Notificacoes;

/// <summary>Push nativo via Firebase Cloud Messaging. O FirebaseApp default é criado no startup.</summary>
public class DispositivoPushService : IDispositivoPushService
{
    private readonly HomePlannerDbContext _db;
    private readonly TenantContext _tenantContext;
    private readonly TenantContextAccessor _tenantAccessor;
    private readonly FcmOptions _options;
    private readonly ILogger<DispositivoPushService> _logger;

    public DispositivoPushService(
        HomePlannerDbContext db,
        TenantContext tenantContext,
        TenantContextAccessor tenantAccessor,
        IOptions<FcmOptions> options,
        ILogger<DispositivoPushService> logger)
    {
        _db = db;
        _tenantContext = tenantContext;
        _tenantAccessor = tenantAccessor;
        _options = options.Value;
        _logger = logger;
    }

    public bool Habilitado => _options.EstaConfigurado;

    public async Task<ResultadoOperacao> RegistrarAsync(RegistrarDispositivoDTO dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Token))
            return ResultadoOperacao.Falha("Token do dispositivo ausente.");

        await _tenantAccessor.GarantirHidratadoAsync();
        var tenantId = _tenantContext.TenantId ?? Guid.Empty;
        var usuarioId = _tenantContext.UsuarioId;
        if (usuarioId is null) return ResultadoOperacao.Falha(ErrosApp.SessaoInvalida);

        // Upsert por token — reaproveita registros soft-deleted (reativa).
        var existente = await _db.DispositivosPush
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.Token == dto.Token, ct);

        if (existente is null)
        {
            _db.DispositivosPush.Add(new DispositivoPush
            {
                TenantId = tenantId,
                UsuarioId = usuarioId,
                Token = dto.Token,
                Plataforma = Truncar(dto.Plataforma, 20),
                DispositivoInfo = Truncar(dto.DispositivoInfo, 512),
                DataCriacao = DateTime.UtcNow,
            });
        }
        else
        {
            existente.TenantId = tenantId;
            existente.UsuarioId = usuarioId;
            existente.Plataforma = Truncar(dto.Plataforma, 20);
            existente.DispositivoInfo = Truncar(dto.DispositivoInfo, 512);
            existente.IsDeleted = false;
            existente.DeletedAt = null;
            existente.DeletedByUsuarioId = null;
        }

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Dispositivo FCM registrado (usuario={UsuarioId}).", usuarioId);
        return ResultadoOperacao.Ok();
    }

    public async Task RemoverAsync(string token, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token)) return;

        var disp = await _db.DispositivosPush.FirstOrDefaultAsync(d => d.Token == token, ct);
        if (disp is null) return;

        disp.IsDeleted = true;
        disp.DeletedAt = DateTime.UtcNow;
        disp.DeletedByUsuarioId = _tenantContext.UsuarioId;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<int> EnviarParaUsuarioAsync(
        Guid tenantId, string usuarioId, NotificacaoPushDTO notificacao, CancellationToken ct = default)
    {
        if (!_options.EstaConfigurado) return 0;

        // IgnoreQueryFilters: pode rodar fora do contexto de requisição (background).
        var dispositivos = await _db.DispositivosPush
            .IgnoreQueryFilters()
            .Where(d => d.TenantId == tenantId && d.UsuarioId == usuarioId && !d.IsDeleted)
            .ToListAsync(ct);

        if (dispositivos.Count == 0) return 0;

        var messaging = FirebaseMessaging.DefaultInstance;
        var enviados = 0;

        foreach (var disp in dispositivos)
        {
            ct.ThrowIfCancellationRequested();
            var mensagem = new Message
            {
                Token = disp.Token,
                Notification = new Notification { Title = notificacao.Titulo, Body = notificacao.Corpo },
                Data = new Dictionary<string, string>
                {
                    ["url"] = notificacao.Url ?? "/",
                    ["tag"] = notificacao.Tag ?? string.Empty,
                },
            };

            try
            {
                await messaging.SendAsync(mensagem, ct);
                disp.UltimoEnvioEm = DateTime.UtcNow;
                enviados++;
            }
            catch (FirebaseMessagingException ex) when (
                ex.MessagingErrorCode is MessagingErrorCode.Unregistered or MessagingErrorCode.InvalidArgument)
            {
                // Token morto/ inválido — remove para não tentar de novo.
                disp.IsDeleted = true;
                disp.DeletedAt = DateTime.UtcNow;
                _logger.LogInformation("Token FCM inválido removido (usuario={UsuarioId}).", usuarioId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao enviar push FCM (usuario={UsuarioId}).", usuarioId);
            }
        }

        await _db.SaveChangesAsync(ct);
        return enviados;
    }

    private static string? Truncar(string? valor, int max)
        => string.IsNullOrEmpty(valor) || valor.Length <= max ? valor : valor[..max];
}
