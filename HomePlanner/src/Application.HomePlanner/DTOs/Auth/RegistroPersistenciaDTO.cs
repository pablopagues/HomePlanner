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
    /// a mesma usada pelo banner de cookies. Carimbada pelo servidor — o cliente não
    /// escolhe qual versão diz ter aceitado.
    /// </summary>
    public string VersaoTermos { get; set; } = LgpdConstants.VersaoAtual;

    /// <summary>
    /// Aceite explícito dos termos e da política de privacidade. Sem isso o registro
    /// é recusado: o `DataAceiteTermos` gravado no usuário é o registro de consentimento
    /// da LGPD, e ele só vale se alguém de fato tiver aceitado.
    /// </summary>
    public bool AceitaTermos { get; set; }
}
