using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Familia;
using Domain.HomePlanner.Models.Enums;

namespace Application.HomePlanner.Services.Familia;

public interface IFamiliaService
{
    Task<IReadOnlyList<MembroFamiliaDetalheDTO>> ListarMembrosAsync(CancellationToken ct = default);

    /// <summary>Uso de assentos: membros ativos vs. limite do plano atual.</summary>
    Task<ResumoFamiliaDTO> ObterResumoAsync(CancellationToken ct = default);

    /// <summary>Cria um novo membro (que poderá logar) e envia o convite de definição de senha.</summary>
    /// <param name="baseUrl">URL base da aplicação (ex.: NavigationManager.BaseUri) para montar o link de convite.</param>
    Task<ResultadoOperacao<ConviteMembroResultadoDTO>> AdicionarMembroAsync(
        NovoMembroDTO dto, string baseUrl, CancellationToken ct = default);

    /// <summary>Reenvia (regenera) o convite de definição de senha de um membro.</summary>
    Task<ResultadoOperacao<ConviteMembroResultadoDTO>> ReenviarConviteAsync(
        string usuarioId, string baseUrl, CancellationToken ct = default);

    Task<ResultadoOperacao> AlterarPapelAsync(string usuarioId, PapelUsuario novoPapel, CancellationToken ct = default);

    Task<ResultadoOperacao> DefinirAtivoAsync(string usuarioId, bool ativo, CancellationToken ct = default);
}
