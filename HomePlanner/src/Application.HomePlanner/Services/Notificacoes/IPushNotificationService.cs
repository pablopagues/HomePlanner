using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Notificacoes;

namespace Application.HomePlanner.Services.Notificacoes;

public interface IPushNotificationService
{
    /// <summary>Verdadeiro quando há chaves VAPID configuradas e o envio está ligado.</summary>
    bool Habilitado { get; }

    /// <summary>Chave pública VAPID (Base64Url) que o navegador usa para assinar. Vazia se desabilitado.</summary>
    string ChavePublica { get; }

    /// <summary>Registra (ou atualiza) a assinatura de push do usuário atual.</summary>
    Task<ResultadoOperacao> RegistrarInscricaoAsync(InscricaoPushDTO dto, CancellationToken ct = default);

    /// <summary>Remove (soft-delete) a assinatura com o endpoint informado.</summary>
    Task RemoverInscricaoAsync(string endpoint, CancellationToken ct = default);

    /// <summary>Verdadeiro se o usuário atual tem ao menos uma assinatura ativa.</summary>
    Task<bool> UsuarioTemInscricaoAsync(CancellationToken ct = default);

    /// <summary>
    /// Envia uma notificação a todas as assinaturas de um usuário. Ignora o filtro de tenant
    /// (usa o tenantId informado) — pode ser chamado fora do contexto de requisição (background).
    /// Retorna quantos envios tiveram sucesso. Assinaturas expiradas (404/410) são removidas.
    /// </summary>
    Task<int> EnviarParaUsuarioAsync(Guid tenantId, string usuarioId, NotificacaoPushDTO notificacao, CancellationToken ct = default);

    /// <summary>Envia uma notificação de teste para o próprio usuário atual.</summary>
    Task<int> EnviarTesteAsync(string titulo, string corpo, CancellationToken ct = default);

    /// <summary>Lembrete de horário de tarefa, no idioma do destinatário.</summary>
    Task<int> EnviarLembreteTarefaAsync(Guid tenantId, string usuarioId, string tituloTarefa, TimeOnly hora, int tarefaId, CancellationToken ct = default);

    /// <summary>Lembrete de horário enviado a um pai (Owner/Membro), identificando o responsável.</summary>
    Task<int> EnviarLembreteTarefaPaisAsync(Guid tenantId, string paiUsuarioId, string nomeResponsavel, string tituloTarefa, TimeOnly hora, int tarefaId, CancellationToken ct = default);

    /// <summary>Aviso de tarefa atribuída, no idioma do destinatário.</summary>
    Task<int> EnviarTarefaAtribuidaAsync(Guid tenantId, string usuarioId, string tituloTarefa, int tarefaId, CancellationToken ct = default);
}
