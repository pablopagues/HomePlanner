using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Seguranca;

namespace Application.HomePlanner.Services.Seguranca;

public interface IDoisFatoresService
{
    /// <summary>True se o usuário atual tem 2FA (app autenticador) ativado.</summary>
    Task<bool> EstaAtivoAsync(CancellationToken ct = default);

    /// <summary>Gera/recupera a chave do autenticador para configuração do app.</summary>
    Task<ResultadoOperacao<ChaveAutenticadorDTO>> GerarChaveAsync(CancellationToken ct = default);

    /// <summary>Verifica o código TOTP, ativa o 2FA e devolve os códigos de recuperação.</summary>
    Task<ResultadoOperacao<IReadOnlyList<string>>> VerificarEAtivarAsync(string codigo, CancellationToken ct = default);

    /// <summary>Gera um novo conjunto de códigos de recuperação (invalida os anteriores).</summary>
    Task<ResultadoOperacao<IReadOnlyList<string>>> GerarNovosCodigosRecuperacaoAsync(CancellationToken ct = default);

    /// <summary>Desativa o 2FA e zera a chave do autenticador.</summary>
    Task<ResultadoOperacao> DesativarAsync(CancellationToken ct = default);
}
