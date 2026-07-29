namespace Application.HomePlanner.DTOs.Cardapio.Receita;

/// <summary>
/// Resolve a URL efetiva da imagem de uma receita. A foto enviada pelo usuário
/// (blob no banco, servida por /receita/{id}/foto) tem precedência sobre a UrlImagem
/// — esta última cobre receitas importadas de sites. O parâmetro de versão faz o
/// cache-busting quando a foto é substituída.
/// </summary>
public static class ReceitaImagemHelper
{
    public static string? ResolverSrc(int receitaId, bool temFoto, DateTime? fotoAtualizadaEm, string? urlImagem)
    {
        if (temFoto && receitaId > 0)
        {
            var versao = (fotoAtualizadaEm ?? DateTime.UnixEpoch).Ticks;
            return $"/receita/{receitaId}/foto?v={versao}";
        }
        return string.IsNullOrWhiteSpace(urlImagem) ? null : urlImagem;
    }
}
