using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Perfil;

namespace Application.HomePlanner.Services.Perfil;

public interface IFotoUsuarioService
{
    /// <summary>Tamanho máximo aceito para a foto, em bytes.</summary>
    long TamanhoMaximoBytes { get; }

    /// <summary>Content-types de imagem aceitos.</summary>
    IReadOnlyList<string> TiposPermitidos { get; }

    /// <summary>Salva (ou substitui) a foto de perfil do usuário atual. Devolve o novo token de versão.</summary>
    Task<ResultadoOperacao<string>> AtualizarFotoAsync(byte[] conteudo, string contentType, CancellationToken ct = default);

    /// <summary>Remove a foto de perfil do usuário atual.</summary>
    Task<ResultadoOperacao> RemoverFotoAsync(CancellationToken ct = default);

    /// <summary>Token de versão da foto do usuário atual, ou null se não houver foto.</summary>
    Task<string?> ObterVersaoFotoAsync(CancellationToken ct = default);

    /// <summary>Conteúdo bruto da foto do usuário atual (para servir via endpoint), ou null.</summary>
    Task<FotoUsuarioDTO?> ObterConteudoFotoAsync(CancellationToken ct = default);

    /// <summary>
    /// Conteúdo bruto da foto de um membro do mesmo tenant (para servir via endpoint), ou null.
    /// O filtro global de tenant garante que só fotos de membros da própria família sejam acessíveis.
    /// </summary>
    Task<FotoUsuarioDTO?> ObterConteudoFotoAsync(string usuarioId, CancellationToken ct = default);
}
