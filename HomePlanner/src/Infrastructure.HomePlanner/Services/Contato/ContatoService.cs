using Application.HomePlanner.Common;
using System.Net;
using Application.HomePlanner.DTOs.Contato;
using Application.HomePlanner.Services.Contato;
using Application.HomePlanner.Services.Email;
using Domain.HomePlanner.Models.SaaS.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.HomePlanner.Services.Contato;

/// <summary>
/// Processa o formulário público de contato.
/// 1) Honeypot → finge sucesso e descarta. 2) Monta e-mail HTML. 3) Envia via IEmailService.
/// </summary>
public class ContatoService : IContatoService
{
    private readonly IEmailService _email;
    private readonly ContatoOptions _opt;
    private readonly ILogger<ContatoService> _logger;

    public ContatoService(IEmailService email, IOptions<ContatoOptions> opt, ILogger<ContatoService> logger)
    {
        _email = email;
        _opt = opt.Value;
        _logger = logger;
    }

    public async Task<ContatoEnvioResultado> EnviarAsync(ContatoMensagemDTO m, CancellationToken ct = default)
    {
        // 1. Honeypot — bots costumam preencher todos os campos
        if (!string.IsNullOrWhiteSpace(m.Website))
        {
            _logger.LogInformation("Mensagem de contato descartada (honeypot). Email={Email}", m.Email);
            return ContatoEnvioResultado.Ok();
        }

        var destino = _opt.EmailDestino;
        if (string.IsNullOrWhiteSpace(destino))
        {
            _logger.LogWarning("Contato:EmailDestino não configurado — mensagem de {Email} não entregue.", m.Email);
            return ContatoEnvioResultado.Falha(ErrosApp.EnvioIndisponivel);
        }

        try
        {
            await _email.EnviarAsync(
                destino,
                $"[Contato HomePlanner] {m.Assunto} — {m.Nome}",
                MontarCorpoHtml(m),
                ct);

            _logger.LogInformation("E-mail de contato enviado. Remetente={Email}, Assunto={Assunto}", m.Email, m.Assunto);
            return ContatoEnvioResultado.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao enviar e-mail de contato. Remetente={Email}", m.Email);
            return ContatoEnvioResultado.Falha(ErrosApp.EnvioIndisponivel);
        }
    }

    private static string MontarCorpoHtml(ContatoMensagemDTO m)
    {
        string E(string? s) => WebUtility.HtmlEncode(s ?? string.Empty);
        string Eml(string? s) => E(s).Replace("\n", "<br>");

        var telefone = string.IsNullOrWhiteSpace(m.Telefone) ? "" :
            $@"<tr><td style=""padding:8px 0;color:#7A8278;font-weight:500;"">Telefone</td>
                   <td style=""padding:8px 0;color:#2A3A33;"">{E(m.Telefone)}</td></tr>";

        return $@"<!DOCTYPE html>
<html lang=""pt-br""><head><meta charset=""utf-8""></head>
<body style=""margin:0;padding:0;background:#FAF7F2;font-family:'Segoe UI',Arial,sans-serif;color:#2A3A33;"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#FAF7F2;padding:30px 0;"">
    <tr><td align=""center"">
      <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 2px 12px rgba(42,58,51,0.08);"">
        <tr><td style=""background:#4A6B5C;padding:24px 32px;"">
          <h1 style=""margin:0;color:#fff;font-size:20px;font-weight:400;"">Nova mensagem de contato</h1>
          <p style=""margin:6px 0 0;color:#dbe7e0;font-size:13px;"">Recebida pelo site HomePlanner</p>
        </td></tr>
        <tr><td style=""padding:28px 32px;"">
          <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""font-size:14px;line-height:1.6;"">
            <tr><td style=""padding:8px 0;color:#7A8278;width:120px;font-weight:500;"">Nome</td>
                <td style=""padding:8px 0;color:#2A3A33;"">{E(m.Nome)}</td></tr>
            <tr><td style=""padding:8px 0;color:#7A8278;font-weight:500;"">E-mail</td>
                <td style=""padding:8px 0;""><a href=""mailto:{E(m.Email)}"" style=""color:#C97B5A;text-decoration:none;"">{E(m.Email)}</a></td></tr>
            {telefone}
            <tr><td style=""padding:8px 0;color:#7A8278;font-weight:500;"">Assunto</td>
                <td style=""padding:8px 0;color:#2A3A33;"">{E(m.Assunto)}</td></tr>
          </table>
          <div style=""margin-top:22px;padding:18px 20px;background:#F5F0EA;border-left:4px solid #C97B5A;border-radius:6px;"">
            <div style=""color:#7A8278;font-size:12px;text-transform:uppercase;letter-spacing:0.5px;margin-bottom:8px;font-weight:600;"">Mensagem</div>
            <div style=""color:#2A3A33;font-size:14px;line-height:1.6;"">{Eml(m.Mensagem)}</div>
          </div>
        </td></tr>
        <tr><td style=""padding:18px 32px;background:#F5F0EA;border-top:1px solid #E8DDD1;color:#9aa39a;font-size:12px;"">
          Enviada via formulário público em {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC.
        </td></tr>
      </table>
    </td></tr>
  </table>
</body></html>";
    }
}
