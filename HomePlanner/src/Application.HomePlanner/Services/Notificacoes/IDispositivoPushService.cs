using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Notificacoes;

namespace Application.HomePlanner.Services.Notificacoes;

/// <summary>
/// Push nativo (FCM) para os apps MAUI. Registro de tokens de aparelho e envio.
/// Convive com o Web Push (VAPID) do site — os dois são disparados pelo mesmo funil de notificações.
/// </summary>
public interface IDispositivoPushService
{
    /// <summary>Verdadeiro quando o FCM está configurado e ligado.</summary>
    bool Habilitado { get; }

    /// <summary>Registra (ou reativa) o token FCM do aparelho para o usuário atual.</summary>
    Task<ResultadoOperacao> RegistrarAsync(RegistrarDispositivoDTO dto, CancellationToken ct = default);

    /// <summary>Remove (soft-delete) o token informado — logout/desativação do aparelho.</summary>
    Task RemoverAsync(string token, CancellationToken ct = default);

    /// <summary>
    /// Envia uma notificação a todos os aparelhos de um usuário via FCM. Ignora o filtro de tenant
    /// (usa o tenantId informado) — pode rodar em background. Devolve quantos envios tiveram sucesso.
    /// Tokens inválidos (Unregistered) são removidos.
    /// </summary>
    Task<int> EnviarParaUsuarioAsync(Guid tenantId, string usuarioId, NotificacaoPushDTO notificacao, CancellationToken ct = default);
}
