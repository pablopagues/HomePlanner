namespace Application.HomePlanner.DTOs.Cardapio.ModeloSemana;

public class ModeloSemanaFiltroDTO
{
    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 20;
    public string? TextoBusca { get; set; }
}
