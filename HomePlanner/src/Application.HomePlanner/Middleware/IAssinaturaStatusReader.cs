using Domain.HomePlanner.Models.Enums;

namespace Application.HomePlanner.Middleware;

/// <summary>Por que o acesso ao produto está bloqueado. Define a mensagem mostrada ao usuário.</summary>
public enum MotivoBloqueio
{
    Nenhum = 0,
    TrialExpirado = 1,
    PagamentoPendente = 2,
    Cancelado = 3,
}

/// <summary>
/// Situação da assinatura de um tenant do ponto de vista de acesso. É derivada das
/// datas a cada leitura — nada é gravado no banco, então não existe janela em que o
/// status persistido esteja "vencido mas ainda liberado" por falta de um job.
/// </summary>
public sealed record EstadoAssinatura(
    MotivoBloqueio Motivo,
    StatusAssinatura Status,
    DateTime? DataFimTrial,
    DateTime? DataExpiracao)
{
    public bool Bloqueada => Motivo != MotivoBloqueio.Nenhum;

    public static readonly EstadoAssinatura Liberada =
        new(MotivoBloqueio.Nenhum, StatusAssinatura.Ativo, null, null);

    /// <summary>
    /// Folga depois do fim do período pago antes de cortar o acesso. Cobre atraso de
    /// webhook e a nova tentativa de cobrança do Stripe — melhor liberar três dias a
    /// mais do que trancar quem pagou.
    /// </summary>
    public const int DiasCarencia = 3;

    /// <summary>Regra de bloqueio. Sem banco e sem relógio implícito, para poder ser testada.</summary>
    public static EstadoAssinatura Avaliar(
        StatusAssinatura status, DateTime? fimTrial, DateTime? expiracao, DateTime agora)
    {
        var motivo = status switch
        {
            // Trial vale até DataFimTrial. Sem data gravada, não bloqueia.
            StatusAssinatura.Trial when fimTrial.HasValue && fimTrial.Value <= agora
                => MotivoBloqueio.TrialExpirado,

            // Cartão recusado ou fatura em aberto no Stripe.
            StatusAssinatura.Suspenso
                => MotivoBloqueio.PagamentoPendente,

            // Cancelado: o acesso segue até o fim do período já pago.
            StatusAssinatura.Cancelado when !expiracao.HasValue || expiracao.Value.AddDays(DiasCarencia) <= agora
                => MotivoBloqueio.Cancelado,

            // Ativo com o período vencido e sem renovação: rede de segurança para webhook perdido.
            StatusAssinatura.Ativo when expiracao.HasValue && expiracao.Value.AddDays(DiasCarencia) <= agora
                => MotivoBloqueio.PagamentoPendente,

            _ => MotivoBloqueio.Nenhum,
        };

        return new EstadoAssinatura(motivo, status, fimTrial, expiracao);
    }
}

/// <summary>
/// Lê a situação de assinatura de um tenant. Interface vive em Application para o
/// middleware usar; implementação em Infrastructure (acessa o DbContext).
/// </summary>
public interface IAssinaturaStatusReader
{
    Task<EstadoAssinatura> ObterAsync(Guid tenantId, CancellationToken ct = default);

    /// <summary>Descarta o valor em cache — chamado quando o Stripe muda a assinatura.</summary>
    void Invalidar(Guid tenantId);
}
