using Application.HomePlanner.Common;

namespace Application.HomePlanner.Services.Auth;

/// <summary>
/// Recuperação e troca de senha. Existe fora das Razor Pages do Identity para que
/// web e app usem exatamente as mesmas regras — o app não tem como reaproveitar
/// um PageModel.
/// </summary>
public interface ISenhaService
{
    /// <summary>
    /// Dispara o e-mail com o link de redefinição.
    ///
    /// Sempre devolve sucesso, exista o e-mail ou não: responder diferente entregaria
    /// quem tem conta no produto. O <paramref name="baseUrl"/> monta o link
    /// (ex.: https://homeplanner.siderisx.ca).
    /// </summary>
    Task<ResultadoOperacao> SolicitarResetAsync(string email, string baseUrl, CancellationToken ct = default);

    /// <summary>Redefine a senha a partir do token recebido por e-mail.</summary>
    Task<ResultadoOperacao> RedefinirAsync(string usuarioId, string token, string novaSenha,
        CancellationToken ct = default);

    /// <summary>Troca a senha do usuário autenticado, exigindo a senha atual.</summary>
    Task<ResultadoOperacao> AlterarAsync(string senhaAtual, string novaSenha, CancellationToken ct = default);
}
