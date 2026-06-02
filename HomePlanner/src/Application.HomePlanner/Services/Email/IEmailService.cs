namespace Application.HomePlanner.Services.Email;

public interface IEmailService
{
    /// <summary>True se há credenciais SMTP configuradas.</summary>
    bool Habilitado { get; }

    Task EnviarAsync(string para, string assunto, string corpoHtml, CancellationToken ct = default);

    Task EnviarConfirmacaoEmailAsync(string para, string nome, string linkConfirmacao, CancellationToken ct = default);

    Task EnviarResetSenhaAsync(string para, string nome, string linkReset, CancellationToken ct = default);
}
