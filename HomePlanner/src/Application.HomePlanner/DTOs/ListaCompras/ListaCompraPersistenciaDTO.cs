namespace Application.HomePlanner.DTOs.ListaCompras;

/// <summary>Dados de criação/edição de uma lista/loja de compras.</summary>
public class ListaCompraPersistenciaDTO
{
    public string Nome { get; set; } = string.Empty;
    public string? Icone { get; set; }
    public string? Cor { get; set; }
}
