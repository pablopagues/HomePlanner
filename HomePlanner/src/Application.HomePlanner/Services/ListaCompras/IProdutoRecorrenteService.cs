using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.ListaCompras;

namespace Application.HomePlanner.Services.ListaCompras;

public interface IProdutoRecorrenteService
{
    /// <summary>Verdadeiro quando o usuário atual pode usar/gerenciar recorrentes (Owner/Membro — não Filho).</summary>
    bool PodeGerenciar { get; }

    /// <summary>Produtos do catálogo, ordenados. Se <paramref name="apenasAtivos"/>, filtra os inativos.</summary>
    Task<IReadOnlyList<ProdutoRecorrenteDTO>> ListarAsync(bool apenasAtivos = false, CancellationToken ct = default);

    Task<ResultadoOperacao<int>> CriarAsync(ProdutoRecorrentePersistenciaDTO dto, CancellationToken ct = default);
    Task<ResultadoOperacao> AtualizarAsync(int id, ProdutoRecorrentePersistenciaDTO dto, CancellationToken ct = default);
    Task<ResultadoOperacao> ExcluirAsync(int id, CancellationToken ct = default);

    /// <summary>Descrições de pedidos passados que ainda não estão no catálogo (para importar em lote).</summary>
    Task<IReadOnlyList<string>> ListarSugestoesHistoricoAsync(CancellationToken ct = default);

    /// <summary>Cria recorrentes em lote a partir de descrições do histórico (sem loja definida).</summary>
    Task<ResultadoOperacao<int>> ImportarDoHistoricoAsync(IReadOnlyCollection<string> descricoes, CancellationToken ct = default);

    /// <summary>
    /// Envia os recorrentes escolhidos para a lista da semana, criando um pedido por item
    /// (herdando loja e quantidade), atribuído ao usuário atual. Pula os que já existem na semana.
    /// </summary>
    Task<ResultadoOperacao<AdicaoRecorrentesResultadoDTO>> AdicionarASemanaAsync(
        IReadOnlyCollection<int> produtoIds, DateOnly dataInicio, CancellationToken ct = default);
}
