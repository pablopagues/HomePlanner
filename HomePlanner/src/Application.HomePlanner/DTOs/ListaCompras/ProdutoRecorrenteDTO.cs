namespace Application.HomePlanner.DTOs.ListaCompras;

/// <summary>Produto do catálogo de recorrentes do tenant.</summary>
public class ProdutoRecorrenteDTO
{
    public int Id { get; init; }
    public string Descricao { get; init; } = string.Empty;
    public string? Quantidade { get; init; }

    /// <summary>Loja onde o produto é comprado (null = balde "Geral").</summary>
    public int? ListaId { get; init; }
    public bool Ativo { get; init; }
    public int Ordem { get; init; }
}
