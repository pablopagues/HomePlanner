using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Assinatura;
using Domain.HomePlanner.Models.Enums;

namespace Application.HomePlanner.Services.Assinatura;

public interface IAssinaturaService
{
    Task<AssinaturaAtualDTO> ObterMinhaAssinaturaAsync(CancellationToken ct = default);

    Task<ResultadoOperacao<StripeRedirectDTO>> IniciarCheckoutAsync(
        PlanoAssinatura plano, string baseUrlAplicacao, CancellationToken ct = default);

    Task<ResultadoOperacao<StripeRedirectDTO>> IniciarGerenciamentoAsync(
        string baseUrlAplicacao, CancellationToken ct = default);
}
