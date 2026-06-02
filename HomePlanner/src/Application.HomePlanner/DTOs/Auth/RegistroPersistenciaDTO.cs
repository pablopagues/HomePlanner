namespace Application.HomePlanner.DTOs.Auth;

public class RegistroPersistenciaDTO
{
    public string NomeResponsavel { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string PaisId { get; set; } = "BR";
    public string VersaoTermos { get; set; } = "1.0";
}
