namespace Application.HomePlanner.DTOs.Cardapio.Ingrediente;

/// <summary>
/// Produto base efetivo de um ingrediente para fins de consolidação na lista de
/// compras. Para um ingrediente sem base, aponta para ele mesmo.
/// </summary>
public record ProdutoBaseInfo(int BaseId, string BaseNome);
