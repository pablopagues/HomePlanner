using Domain.HomePlanner.Models.Enums;

namespace Application.HomePlanner.DTOs.Cardapio.Cardapio;

/// <summary>
/// Move o prato de um slot para outro dentro da mesma semana. Se o destino estiver
/// ocupado, os dois trocam de lugar.
/// </summary>
public class MoverRefeicaoCommand
{
    public DateOnly DataInicio { get; set; }
    public DiaSemana DiaOrigem { get; set; }
    public TipoRefeicao TipoOrigem { get; set; }
    public DiaSemana DiaDestino { get; set; }
    public TipoRefeicao TipoDestino { get; set; }
}
