namespace Application.HomePlanner.DTOs.Seguranca;

public class ChaveAutenticadorDTO
{
    /// <summary>Chave base32 sem formatação — usada na URI otpauth para o QR code.</summary>
    public string ChaveRaw { get; init; } = string.Empty;

    /// <summary>Chave agrupada em blocos de 4 para digitação manual.</summary>
    public string ChaveFormatada { get; init; } = string.Empty;

    /// <summary>E-mail do usuário — rótulo da conta no app autenticador.</summary>
    public string Email { get; init; } = string.Empty;
}
