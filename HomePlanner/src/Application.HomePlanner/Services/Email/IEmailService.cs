namespace Application.HomePlanner.Services.Email;

public interface IEmailService
{
    /// <summary>True se há credenciais SMTP configuradas.</summary>
    bool Habilitado { get; }

    Task EnviarAsync(string para, string assunto, string corpoHtml, CancellationToken ct = default);

    Task EnviarConfirmacaoEmailAsync(string para, string nome, string linkConfirmacao, CancellationToken ct = default);

    Task EnviarResetSenhaAsync(string para, string nome, string linkReset, CancellationToken ct = default);

    /// <summary>
    /// Convite para um novo membro da família definir a senha e entrar.
    /// Distinto do reset de senha: quem recebe ainda não conhece o produto, então o texto
    /// precisa explicar quem convidou e o que é o HomePlanner.
    /// </summary>
    /// <param name="nomeConvidante">Quem enviou o convite (aparece no corpo do e-mail).</param>
    /// <param name="diasValidade">Validade do link, informada ao convidado.</param>
    Task EnviarConviteMembroAsync(string para, string nome, string nomeConvidante,
        string linkConvite, int diasValidade, CancellationToken ct = default);
}
