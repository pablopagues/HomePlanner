namespace Application.HomePlanner.DTOs.ListaCompras;

/// <summary>Resultado de enviar produtos recorrentes para a lista da semana.</summary>
public class AdicaoRecorrentesResultadoDTO
{
    /// <summary>Quantos viraram pedido novo na semana.</summary>
    public int Adicionados { get; init; }

    /// <summary>Quantos foram pulados por já existirem na lista da semana.</summary>
    public int Pulados { get; init; }
}
