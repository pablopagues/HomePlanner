using Domain.HomePlanner.Models.Enums;

namespace Application.HomePlanner.DTOs.Empresa;

public class EmpresaDetalheDTO
{
    public string NomeResponsavel { get; set; } = string.Empty;

    // E-mail de login (somente leitura) + status de confirmação
    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmado { get; set; }

    // País determina os preços da assinatura — não editável aqui
    public string PaisId { get; set; } = Paises.Brasil;

    // ─── Brasil ───
    public string? Cpf { get; set; }

    // ─── Canadá ───
    public string? Province { get; set; }
}
