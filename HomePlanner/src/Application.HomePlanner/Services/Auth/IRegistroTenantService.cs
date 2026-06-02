using Application.HomePlanner.DTOs.Auth;

namespace Application.HomePlanner.Services.Auth;

public interface IRegistroTenantService
{
    /// <summary>
    /// Cria atomicamente: Tenant + Papéis (Owner/Membro/Filho) + Usuário (Owner) +
    /// ConfiguracaoAssinatura (trial). Tudo numa transação — falha = rollback total.
    /// </summary>
    Task<RegistroResultadoDTO> RegistrarAsync(RegistroPersistenciaDTO dto, CancellationToken ct = default);
}
