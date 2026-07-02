using MudBlazor;

namespace HomePlanner.BlazorServer.Components.Shared;

/// <summary>
/// Conjunto fixo de ícones que uma lista/loja de compras pode usar. Guardamos só a chave
/// (ex.: "Store") no banco e resolvemos aqui para o path SVG do MudBlazor.
/// </summary>
public static class IconesLoja
{
    public static readonly IReadOnlyDictionary<string, string> Presets = new Dictionary<string, string>
    {
        ["Store"]             = Icons.Material.Filled.Store,
        ["ShoppingCart"]      = Icons.Material.Filled.ShoppingCart,
        ["LocalGroceryStore"] = Icons.Material.Filled.LocalGroceryStore,
        ["LocalPharmacy"]     = Icons.Material.Filled.LocalPharmacy,
        ["Fastfood"]          = Icons.Material.Filled.Fastfood,
        ["Liquor"]            = Icons.Material.Filled.Liquor,
        ["Pets"]              = Icons.Material.Filled.Pets,
        ["Hardware"]          = Icons.Material.Filled.Hardware,
        ["Checkroom"]         = Icons.Material.Filled.Checkroom,
        ["Redeem"]            = Icons.Material.Filled.Redeem,
    };

    public const string Padrao = "Store";

    /// <summary>Resolve a chave para o ícone MudBlazor; cai no ícone padrão se desconhecida.</summary>
    public static string Resolver(string? chave)
        => chave is not null && Presets.TryGetValue(chave, out var icone)
            ? icone
            : Icons.Material.Filled.Store;
}
