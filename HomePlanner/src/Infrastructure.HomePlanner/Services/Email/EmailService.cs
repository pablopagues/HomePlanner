using Application.HomePlanner.Services.Email;
using Domain.HomePlanner.Models.SaaS.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Resources.HomePlanner;

namespace Infrastructure.HomePlanner.Services.Email;

public class EmailService : IEmailService
{
    private readonly GmailOptions _opts;
    private readonly ILogger<EmailService> _logger;
    // Textos no idioma da requisição/circuito de quem dispara o envio.
    private readonly IStringLocalizer<SharedResource> _l;

    public EmailService(IOptions<GmailOptions> opts, ILogger<EmailService> logger, IStringLocalizer<SharedResource> l)
    {
        _opts = opts.Value;
        _logger = logger;
        _l = l;
    }

    public bool Habilitado =>
        !string.IsNullOrWhiteSpace(_opts.Email) && !string.IsNullOrWhiteSpace(_opts.Password);

    public async Task EnviarAsync(string para, string assunto, string corpoHtml, CancellationToken ct = default)
    {
        if (!Habilitado)
        {
            // Modo dev: registra no log em vez de enviar
            _logger.LogWarning("[EMAIL DESABILITADO] Para: {Para} | Assunto: {Assunto}\n{Corpo}",
                para, assunto, corpoHtml);
            return;
        }

        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(_opts.NomeRemetente, _opts.Email));
        msg.To.Add(MailboxAddress.Parse(para));
        msg.Subject = assunto;
        msg.Body = new BodyBuilder { HtmlBody = corpoHtml }.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_opts.Host, _opts.Port,
            _opts.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto, ct);
        await client.AuthenticateAsync(_opts.Email, _opts.Password, ct);
        await client.SendAsync(msg, ct);
        await client.DisconnectAsync(true, ct);

        _logger.LogInformation("E-mail enviado para {Para}: {Assunto}", para, assunto);
    }

    public Task EnviarConfirmacaoEmailAsync(string para, string nome, string linkConfirmacao, CancellationToken ct = default)
    {
        var corpo = Template(
            _l["Email_Ola", nome],
            _l["Email_ConfirmacaoTexto"],
            _l["Email_ConfirmacaoBotao"], linkConfirmacao);
        return EnviarAsync(para, _l["Email_ConfirmacaoAssunto"], corpo, ct);
    }

    public Task EnviarResetSenhaAsync(string para, string nome, string linkReset, CancellationToken ct = default)
    {
        var corpo = Template(
            _l["Email_Ola", nome],
            _l["Email_ResetTexto"],
            _l["Email_ResetBotao"], linkReset);
        return EnviarAsync(para, _l["Email_ResetAssunto"], corpo, ct);
    }

    public Task EnviarConviteMembroAsync(string para, string nome, string nomeConvidante,
        string linkConvite, int diasValidade, CancellationToken ct = default)
    {
        var convidante = string.IsNullOrWhiteSpace(nomeConvidante) ? _l["Email_SuaFamilia"].Value : nomeConvidante;
        var corpo = Template(
            _l["Email_Ola", nome],
            _l["Email_ConviteTexto", convidante],
            _l["Email_ConviteBotao"], linkConvite,
            _l["Email_ConviteValidade", diasValidade, convidante]);
        return EnviarAsync(para, _l["Email_ConviteAssunto", convidante], corpo, ct);
    }

    private static string Template(string titulo, string texto, string textoBotao, string link,
        string? observacao = null)
    {
        var blocoObservacao = observacao is null
            ? string.Empty
            : $"""<p style="font-size:13px; line-height:1.5; color:#7A8278; margin-top:20px;">{observacao}</p>""";

        return $$"""
            <div style="font-family:'Segoe UI',Arial,sans-serif; max-width:480px; margin:0 auto; background:#FAF7F2; padding:32px; border-radius:12px; color:#2A3A33;">
                <h1 style="font-weight:300; color:#4A6B5C; font-size:24px;">{{titulo}}</h1>
                <p style="font-size:15px; line-height:1.6; color:#2A3A33;">{{texto}}</p>
                <a href="{{link}}" style="display:inline-block; margin-top:16px; background:#C97B5A; color:#FFFFFF; padding:12px 28px; border-radius:8px; text-decoration:none; font-weight:500;">{{textoBotao}}</a>
                {{blocoObservacao}}
                <p style="font-size:12px; color:#7A8278; margin-top:32px;">HomePlanner — by SiderisX</p>
            </div>
            """;
    }
}
