using Microsoft.AspNetCore.Http;

namespace Application.HomePlanner.Middleware;

/// <summary>
/// Barra o acesso ao produto quando o trial venceu ou a assinatura não está em dia.
/// O usuário continua logado e mantém perfil, configurações e a tela de planos —
/// só o restante do sistema fica fora do ar até ele contratar.
/// Roda DEPOIS de UseTenantContext e UseOnboardingRequired, ANTES de UseAuthorization.
/// </summary>
public class AssinaturaRequiredMiddleware
{
    private readonly RequestDelegate _next;

    public AssinaturaRequiredMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(
        HttpContext context,
        TenantContext tenantContext,
        IAssinaturaStatusReader statusReader)
    {
        // Não autenticado → segue (inclui o webhook do Stripe, que é anônimo)
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        // Sem tenant → segue (estado anômalo tratado em outro lugar)
        if (tenantContext.TenantId is null)
        {
            await _next(context);
            return;
        }

        var path = context.Request.Path.Value ?? string.Empty;
        if (AssinaturaRotas.Liberada(path))
        {
            await _next(context);
            return;
        }

        var estado = await statusReader.ObterAsync(tenantContext.TenantId.Value, context.RequestAborted);
        if (!estado.Bloqueada)
        {
            await _next(context);
            return;
        }

        // API mobile: redirecionar JSON não ajuda o app — devolvemos 402 com o motivo.
        // O campo "erros" repete a mensagem no formato padrão da API para o cliente
        // atual exibir algo útil; "erro"/"motivo" são para o app reagir por código.
        if (path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status402PaymentRequired;
            await context.Response.WriteAsJsonAsync(
                new
                {
                    erro = "assinatura_bloqueada",
                    motivo = estado.Motivo.ToString(),
                    erros = new[] { MensagemDoMotivo(estado.Motivo) },
                },
                context.RequestAborted);
            return;
        }

        // Owner resolve na própria tela de planos; os demais só podem avisar o responsável.
        context.Response.Redirect(context.User.IsInRole("Owner")
            ? AssinaturaRotas.DestinoOwner
            : AssinaturaRotas.DestinoMembro);
    }

    /// <summary>
    /// Mensagem para o app. Aponta para a web porque Apple e Google exigem
    /// In-App Purchase para venda de assinatura dentro do app — a contratação
    /// não acontece no celular.
    /// </summary>
    private static string MensagemDoMotivo(MotivoBloqueio motivo) => motivo switch
    {
        MotivoBloqueio.TrialExpirado =>
            "Seu período de teste terminou. Escolha um plano na versão web para continuar.",
        MotivoBloqueio.PagamentoPendente =>
            "Há um pagamento pendente. Regularize na versão web para continuar.",
        MotivoBloqueio.Cancelado =>
            "Sua assinatura foi cancelada. Escolha um plano na versão web para continuar.",
        _ => "Assinatura inativa.",
    };
}
