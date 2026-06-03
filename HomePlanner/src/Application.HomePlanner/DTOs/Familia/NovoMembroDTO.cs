using Domain.HomePlanner.Models.Enums;

namespace Application.HomePlanner.DTOs.Familia;

public class NovoMembroDTO
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    /// <summary>Pai/responsável = Membro; filho = Filho. Owner não é atribuível aqui.</summary>
    public PapelUsuario Papel { get; set; } = PapelUsuario.Membro;
}
