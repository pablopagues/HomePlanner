using Domain.HomePlanner.Models.SaaS.Interfaces;

namespace Domain.HomePlanner.Models.Cardapio;

/// <summary>
/// Loja padrão "aprendida" para um ingrediente do cardápio (global do tenant, não semanal).
/// Ao mover um item do cardápio para uma loja, grava-se aqui a preferência; nas próximas
/// semanas o item já aparece na loja certa automaticamente. É a fonte de verdade da loja
/// dos itens do cardápio (que não são persistidos individualmente).
/// </summary>
public class PreferenciaLojaIngrediente : ITenantEntity, IAuditable
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }

    /// <summary>Ingrediente (produto base) ao qual a preferência se aplica.</summary>
    public int IngredienteId { get; set; }

    /// <summary>Lista/loja preferida para este ingrediente.</summary>
    public int ListaId { get; set; }

    // IAuditable
    public DateTime DataCriacao { get; set; }
    public DateTime? DataModificacao { get; set; }
    public string? CriadoPor { get; set; }
    public string? ModificadoPor { get; set; }

    // Navigation
    public ListaCompra? Lista { get; set; }
}
