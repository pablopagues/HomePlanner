using Application.HomePlanner.Lgpd;

namespace Application.HomePlanner.DTOs.Auth;

public class RegistroPersistenciaDTO
{
    public string NomeResponsavel { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string PaisId { get; set; } = "BR";

    /// <summary>
    /// Versão dos termos aceita no registro. Fonte única: <see cref="LgpdConstants.VersaoAtual"/>,
    /// a mesma usada pelo banner de cookies.
    /// </summary>
    public string VersaoTermos { get; set; } = LgpdConstants.VersaoAtual;
}
