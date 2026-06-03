using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Empresa;

namespace Application.HomePlanner.Services.Empresa;

public interface IEmpresaService
{
    /// <summary>Dados do tenant + e-mail/status de confirmação do usuário atual.</summary>
    Task<EmpresaDetalheDTO> ObterAsync(CancellationToken ct = default);

    /// <summary>Atualiza os dados editáveis do tenant. Retorna a lista de erros (vazia em caso de sucesso).</summary>
    Task<List<string>> SalvarDadosAsync(AtualizarEmpresaDTO dto, CancellationToken ct = default);

    /// <summary>
    /// Reenvia o e-mail de confirmação para o usuário atual.
    /// <paramref name="baseUrl"/> é a URL base da aplicação (ex.: NavigationManager.BaseUri),
    /// usada para montar o link de confirmação.
    /// </summary>
    Task<ResultadoOperacao> ReenviarConfirmacaoEmailAsync(string baseUrl, CancellationToken ct = default);
}
