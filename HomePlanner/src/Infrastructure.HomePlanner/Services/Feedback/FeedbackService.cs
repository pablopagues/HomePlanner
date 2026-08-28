using Application.HomePlanner.Common;
using System.Net;
using System.Reflection;
using Application.HomePlanner.DTOs.Feedback;
using Application.HomePlanner.Middleware;
using Application.HomePlanner.Services.Email;
using Application.HomePlanner.Services.Feedback;
using Domain.HomePlanner.Models.SaaS.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.HomePlanner.Services.Feedback;

/// <summary>
/// Recebe o feedback in-app, identifica o remetente pelo TenantContext e envia
/// e-mail para a equipe (mesmo destino do formulário público de contato).
/// </summary>
public class FeedbackService : IFeedbackService
{
    private readonly IEmailService _email;
    private readonly TenantContext _tenantContext;
    private readonly TenantContextAccessor _tenantAccessor;
    private readonly ContatoOptions _opt;
    private readonly ILogger<FeedbackService> _logger;

    public FeedbackService(
        IEmailService email,
        TenantContext tenantContext,
        TenantContextAccessor tenantAccessor,
        IOptions<ContatoOptions> opt,
        ILogger<FeedbackService> logger)
    {
        _email = email;
        _tenantContext = tenantContext;
        _tenantAccessor = tenantAccessor;
        _opt = opt.Value;
        _logger = logger;
    }

    public async Task<FeedbackEnvioResultado> EnviarAsync(FeedbackDTO dto, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        var destino = _opt.EmailDestino;
        if (string.IsNullOrWhiteSpace(destino))
        {
            _logger.LogWarning("Feedback: EmailDestino não configurado — feedback descartado.");
            return FeedbackEnvioResultado.Falha(ErrosApp.EnvioIndisponivel);
        }

        var nomeUsuario = string.IsNullOrWhiteSpace(_tenantContext.UsuarioNome)
            ? "(usuário não identificado)" : _tenantContext.UsuarioNome;
        var usuarioId = _tenantContext.UsuarioId ?? "";
        var tenantId = _tenantContext.TenantId?.ToString() ?? "(sem tenant)";
        var versaoApp = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "(versão desconhecida)";
        var paginaAtual = string.IsNullOrWhiteSpace(dto.PaginaAtual) ? "(não informada)" : dto.PaginaAtual;
        var tipoStr = TraduzirTipo(dto.Tipo);

        try
        {
            await _email.EnviarAsync(
                destino,
                $"[Feedback {tipoStr}] HomePlanner — {nomeUsuario}",
                MontarCorpoHtml(dto, tipoStr, nomeUsuario, usuarioId, tenantId, paginaAtual, versaoApp),
                ct);

            _logger.LogInformation(
                "Feedback enviado: tipo={Tipo}, usuário={Usuario}, tenant={Tenant}",
                tipoStr, nomeUsuario, tenantId);

            return FeedbackEnvioResultado.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao enviar e-mail de feedback (tipo {Tipo}, usuário {Usuario})",
                tipoStr, nomeUsuario);
            return FeedbackEnvioResultado.Falha(ErrosApp.EnvioIndisponivel);
        }
    }

    private static string TraduzirTipo(TipoFeedback tipo) => tipo switch
    {
        TipoFeedback.Bug => "Bug",
        TipoFeedback.Sugestao => "Sugestão",
        TipoFeedback.Elogio => "Elogio",
        TipoFeedback.Outro => "Outro",
        _ => tipo.ToString(),
    };

    private static string MontarCorpoHtml(
        FeedbackDTO dto, string tipoStr, string nomeUsuario, string usuarioId,
        string tenantId, string paginaAtual, string versaoApp)
    {
        string E(string? s) => WebUtility.HtmlEncode(s ?? string.Empty);
        var mensagemSafe = E(dto.Mensagem).Replace("\r\n", "<br>").Replace("\n", "<br>");

        // Cor do badge varia por tipo: bug vermelho, elogio verde, sugestão sage, outro neutro.
        var corTipo = dto.Tipo switch
        {
            TipoFeedback.Bug => "#C0564A",
            TipoFeedback.Elogio => "#5B8A6A",
            TipoFeedback.Sugestao => "#4A6B5C",
            _ => "#7A8278",
        };

        return $@"<!DOCTYPE html>
<html lang=""pt-br""><head><meta charset=""utf-8""></head>
<body style=""margin:0;padding:0;background:#FAF7F2;font-family:'Segoe UI',Arial,sans-serif;color:#2A3A33;"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#FAF7F2;padding:30px 0;"">
    <tr><td align=""center"">
      <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 2px 12px rgba(42,58,51,0.08);"">
        <tr><td style=""background:#4A6B5C;padding:24px 32px;"">
          <h1 style=""margin:0;color:#fff;font-size:20px;font-weight:400;"">Novo feedback recebido</h1>
          <p style=""margin:6px 0 0;color:#dbe7e0;font-size:13px;"">Enviado de dentro do app HomePlanner</p>
        </td></tr>
        <tr><td style=""padding:28px 32px;"">
          <p style=""margin:0 0 16px;"">
            <span style=""background:{corTipo};color:#fff;padding:4px 12px;border-radius:6px;font-size:13px;font-weight:600;"">{E(tipoStr)}</span>
          </p>
          <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""font-size:14px;line-height:1.6;"">
            <tr><td style=""padding:8px 0;color:#7A8278;width:120px;font-weight:500;"">Usuário</td>
                <td style=""padding:8px 0;color:#2A3A33;""><strong>{E(nomeUsuario)}</strong> <span style=""color:#9aa39a;font-size:12px;"">({E(usuarioId)})</span></td></tr>
            <tr><td style=""padding:8px 0;color:#7A8278;font-weight:500;"">Tenant</td>
                <td style=""padding:8px 0;color:#2A3A33;"">{E(tenantId)}</td></tr>
            <tr><td style=""padding:8px 0;color:#7A8278;font-weight:500;"">Página</td>
                <td style=""padding:8px 0;color:#2A3A33;""><code>{E(paginaAtual)}</code></td></tr>
            <tr><td style=""padding:8px 0;color:#7A8278;font-weight:500;"">Versão</td>
                <td style=""padding:8px 0;color:#2A3A33;""><code>{E(versaoApp)}</code></td></tr>
          </table>
          <div style=""margin-top:22px;padding:18px 20px;background:#F5F0EA;border-left:4px solid {corTipo};border-radius:6px;"">
            <div style=""color:#7A8278;font-size:12px;text-transform:uppercase;letter-spacing:0.5px;margin-bottom:8px;font-weight:600;"">Mensagem</div>
            <div style=""color:#2A3A33;font-size:14px;line-height:1.6;"">{mensagemSafe}</div>
          </div>
        </td></tr>
        <tr><td style=""padding:18px 32px;background:#F5F0EA;border-top:1px solid #E8DDD1;color:#9aa39a;font-size:12px;"">
          Recebido em {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC.
        </td></tr>
      </table>
    </td></tr>
  </table>
</body></html>";
    }
}
