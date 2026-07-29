namespace Application.HomePlanner.DTOs.ListaCompras;

/// <summary>Dados de criação/edição de um produto recorrente do catálogo.</summary>
public class ProdutoRecorrentePersistenciaDTO
{
    public string Descricao { get; set; } = string.Empty;
    public string? Quantidade { get; set; }

    /// <summary>Loja onde o produto é comprado (null = balde "Geral").</summary>
    public int? ListaId { get; set; }
    public bool Ativo { get; set; } = true;
}
