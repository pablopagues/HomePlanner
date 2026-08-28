using Domain.HomePlanner.Models.Enums;

namespace Application.HomePlanner.DTOs.Familia;

public class MembroFamiliaDetalheDTO
{
    public string UsuarioId { get; init; } = string.Empty;
    public string Nome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public PapelUsuario Papel { get; init; }
    public bool Ativo { get; init; }
    public bool SenhaDefinida { get; init; }
    public DateTime? UltimoLogin { get; init; }

    /// <summary>Quando o convite foi enviado pela última vez (UTC). Null enquanto nunca foi enviado
    /// ou para membros criados antes do rastreio existir.</summary>
    public DateTime? UltimoConviteEnviadoEm { get; init; }
}
