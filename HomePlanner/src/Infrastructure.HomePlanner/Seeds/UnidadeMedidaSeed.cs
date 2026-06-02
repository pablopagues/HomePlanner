using Domain.HomePlanner.Models.Cardapio;
using Domain.HomePlanner.Models.Enums;

namespace Infrastructure.HomePlanner.Seeds;

public static class UnidadeMedidaSeed
{
    public static IReadOnlyList<UnidadeMedida> Obter() =>
    [
        new() { Id = 1,  Codigo = "g",      Nome = "Grama",          ChaveTraducao = "UnidadeMedida_Grama",       Tipo = TipoUnidadeMedida.Massa,    FatorParaBase = 1m },
        new() { Id = 2,  Codigo = "kg",     Nome = "Quilograma",     ChaveTraducao = "UnidadeMedida_Quilograma",  Tipo = TipoUnidadeMedida.Massa,    FatorParaBase = 1000m },
        new() { Id = 3,  Codigo = "ml",     Nome = "Mililitro",      ChaveTraducao = "UnidadeMedida_Mililitro",   Tipo = TipoUnidadeMedida.Volume,   FatorParaBase = 1m },
        new() { Id = 4,  Codigo = "l",      Nome = "Litro",          ChaveTraducao = "UnidadeMedida_Litro",       Tipo = TipoUnidadeMedida.Volume,   FatorParaBase = 1000m },
        new() { Id = 5,  Codigo = "xic",    Nome = "Xícara (240ml)", ChaveTraducao = "UnidadeMedida_Xicara",      Tipo = TipoUnidadeMedida.Volume,   FatorParaBase = 240m },
        new() { Id = 6,  Codigo = "cs",     Nome = "Colher de sopa", ChaveTraducao = "UnidadeMedida_ColherSopa",  Tipo = TipoUnidadeMedida.Volume,   FatorParaBase = 15m },
        new() { Id = 7,  Codigo = "cc",     Nome = "Colher de chá",  ChaveTraducao = "UnidadeMedida_ColherCha",   Tipo = TipoUnidadeMedida.Volume,   FatorParaBase = 5m },
        new() { Id = 8,  Codigo = "pitada", Nome = "Pitada",         ChaveTraducao = "UnidadeMedida_Pitada",      Tipo = TipoUnidadeMedida.Volume,   FatorParaBase = 0.3m },
        new() { Id = 9,  Codigo = "un",     Nome = "Unidade",        ChaveTraducao = "UnidadeMedida_Unidade",     Tipo = TipoUnidadeMedida.Unidade,  FatorParaBase = 1m },
        new() { Id = 10, Codigo = "dente",  Nome = "Dente",          ChaveTraducao = "UnidadeMedida_Dente",       Tipo = TipoUnidadeMedida.Unidade,  FatorParaBase = 1m },
        new() { Id = 11, Codigo = "fatia",  Nome = "Fatia",          ChaveTraducao = "UnidadeMedida_Fatia",       Tipo = TipoUnidadeMedida.Unidade,  FatorParaBase = 1m },
        new() { Id = 12, Codigo = "pacote", Nome = "Pacote",         ChaveTraducao = "UnidadeMedida_Pacote",      Tipo = TipoUnidadeMedida.Unidade,  FatorParaBase = 1m },
    ];
}
