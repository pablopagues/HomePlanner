using System.Net.Http.Json;

namespace HomePlanner.MobileApp.Services;

/// <summary>
/// Cliente autenticado da API HomePlanner. Passa pelo AuthMessageHandler (Bearer + refresh).
/// URLs são absolutas, compostas a partir de <see cref="SessaoAtual.BaseUrl"/>.
/// </summary>
public class ApiClient
{
    private readonly HttpClient _http;
    private readonly SessaoAtual _sessao;

    public ApiClient(HttpClient http, SessaoAtual sessao)
    {
        _http = http;
        _sessao = sessao;
    }

    private string Url(string caminho) => $"{_sessao.BaseUrl.TrimEnd('/')}{caminho}";

    // ── Helpers genéricos ─────────────────────────────────────────────────
    private async Task<(T? dados, string? erro)> GetAsync<T>(string caminho, CancellationToken ct)
    {
        try
        {
            var resp = await _http.GetAsync(Url(caminho), ct);
            if (resp.IsSuccessStatusCode)
                return (await resp.Content.ReadFromJsonAsync<T>(cancellationToken: ct), null);
            return (default, await ErroApi.LerAsync(resp, ct));
        }
        catch (Exception ex) { return (default, $"Falha de conexão: {ex.Message}"); }
    }

    private async Task<string?> EnviarAsync(HttpMethod metodo, string caminho, object? corpo, CancellationToken ct)
    {
        try
        {
            using var req = new HttpRequestMessage(metodo, Url(caminho));
            if (corpo is not null) req.Content = JsonContent.Create(corpo);
            var resp = await _http.SendAsync(req, ct);
            return resp.IsSuccessStatusCode ? null : await ErroApi.LerAsync(resp, ct);
        }
        catch (Exception ex) { return $"Falha de conexão: {ex.Message}"; }
    }

    private async Task<(T? dados, string? erro)> EnviarAsync<T>(HttpMethod metodo, string caminho, object? corpo, CancellationToken ct)
    {
        try
        {
            using var req = new HttpRequestMessage(metodo, Url(caminho));
            if (corpo is not null) req.Content = JsonContent.Create(corpo);
            var resp = await _http.SendAsync(req, ct);
            if (resp.IsSuccessStatusCode)
                return (await resp.Content.ReadFromJsonAsync<T>(cancellationToken: ct), null);
            return (default, await ErroApi.LerAsync(resp, ct));
        }
        catch (Exception ex) { return (default, $"Falha de conexão: {ex.Message}"); }
    }

    // ── Cardápio ──────────────────────────────────────────────────────────
    public Task<(CardapioSemanaDTO? dados, string? erro)> ObterCardapioSemanaAsync(DateOnly segunda, CancellationToken ct = default)
        => GetAsync<CardapioSemanaDTO>($"/api/cardapio/semana/{segunda:yyyy-MM-dd}", ct);

    public Task<string?> DefinirRefeicaoAsync(DefinirRefeicaoRequest cmd, CancellationToken ct = default)
        => EnviarAsync(HttpMethod.Post, "/api/cardapio/refeicao", cmd, ct);

    public Task<(CardapioSemanaDTO? dados, string? erro)> CopiarSemanaAsync(DateOnly origem, DateOnly destino, CancellationToken ct = default)
        => EnviarAsync<CardapioSemanaDTO>(HttpMethod.Post, $"/api/cardapio/copiar?origem={origem:yyyy-MM-dd}&destino={destino:yyyy-MM-dd}", null, ct);

    // ── Receitas ──────────────────────────────────────────────────────────
    public Task<(ResultadoListagem<ReceitaListaDTO>? dados, string? erro)> ListarReceitasAsync(string? busca, int pagina = 1, CancellationToken ct = default)
        => GetAsync<ResultadoListagem<ReceitaListaDTO>>($"/api/receitas?pagina={pagina}&tamanhoPagina=20&textoBusca={Uri.EscapeDataString(busca ?? "")}", ct);

    public Task<(ReceitaDetalheDTO? dados, string? erro)> ObterReceitaAsync(int id, CancellationToken ct = default)
        => GetAsync<ReceitaDetalheDTO>($"/api/receitas/{id}", ct);

    public Task<string?> DeletarReceitaAsync(int id, CancellationToken ct = default)
        => EnviarAsync(HttpMethod.Delete, $"/api/receitas/{id}", null, ct);

    public Task<(int dados, string? erro)> DuplicarReceitaAsync(int id, CancellationToken ct = default)
        => EnviarAsync<int>(HttpMethod.Post, $"/api/receitas/{id}/duplicar", null, ct);

    public Task<(int dados, string? erro)> SalvarReceitaAsync(ReceitaPersistenciaRequest dto, CancellationToken ct = default)
        => EnviarAsync<int>(HttpMethod.Post, "/api/receitas", dto, ct);

    // ── Ingredientes ──────────────────────────────────────────────────────
    public Task<(ResultadoListagem<IngredienteListaDTO>? dados, string? erro)> ListarIngredientesAsync(string? busca, int pagina = 1, CancellationToken ct = default)
        => GetAsync<ResultadoListagem<IngredienteListaDTO>>($"/api/ingredientes?pagina={pagina}&tamanhoPagina=20&textoBusca={Uri.EscapeDataString(busca ?? "")}", ct);

    // ── Planner ───────────────────────────────────────────────────────────
    public Task<(ResultadoListagem<TarefaListaDTO>? dados, string? erro)> ListarTarefasAsync(bool? concluida, int pagina = 1, CancellationToken ct = default)
        => GetAsync<ResultadoListagem<TarefaListaDTO>>($"/api/planner?pagina={pagina}&tamanhoPagina=50{(concluida.HasValue ? $"&concluida={concluida}" : "")}", ct);

    public Task<string?> ConcluirTarefaAsync(int id, bool concluida, CancellationToken ct = default)
        => EnviarAsync(HttpMethod.Post, $"/api/planner/{id}/concluir?concluida={concluida}", null, ct);

    public Task<string?> DeletarTarefaAsync(int id, CancellationToken ct = default)
        => EnviarAsync(HttpMethod.Delete, $"/api/planner/{id}", null, ct);

    public Task<(int dados, string? erro)> SalvarTarefaAsync(TarefaPersistenciaRequest dto, CancellationToken ct = default)
        => EnviarAsync<int>(HttpMethod.Post, "/api/planner", dto, ct);

    public Task<(List<MembroSimplesDTO>? dados, string? erro)> MembrosPlannerAsync(CancellationToken ct = default)
        => GetAsync<List<MembroSimplesDTO>>("/api/planner/membros", ct);

    // ── Compras ───────────────────────────────────────────────────────────
    public Task<(ListaComprasDTO? dados, string? erro)> ComprasDaSemanaAsync(DateOnly segunda, CancellationToken ct = default)
        => GetAsync<ListaComprasDTO>($"/api/compras/semana/{segunda:yyyy-MM-dd}", ct);

    public Task<(Dictionary<int, bool>? dados, string? erro)> MarcacoesSemanaAsync(DateOnly segunda, CancellationToken ct = default)
        => GetAsync<Dictionary<int, bool>>($"/api/compras/semana/{segunda:yyyy-MM-dd}/marcacoes", ct);

    public Task<string?> MarcarItemAsync(DateOnly segunda, int ingredienteId, bool comprado, CancellationToken ct = default)
        => EnviarAsync(HttpMethod.Post, $"/api/compras/semana/{segunda:yyyy-MM-dd}/marcar?ingredienteId={ingredienteId}&comprado={comprado}", null, ct);

    public Task<(List<PedidoMembroGrupoDTO>? dados, string? erro)> PedidosDaSemanaAsync(DateOnly segunda, CancellationToken ct = default)
        => GetAsync<List<PedidoMembroGrupoDTO>>($"/api/compras/semana/{segunda:yyyy-MM-dd}/pedidos", ct);

    public Task<string?> MarcarPedidoAsync(int id, bool comprado, CancellationToken ct = default)
        => EnviarAsync(HttpMethod.Post, $"/api/compras/pedidos/{id}/comprado?comprado={comprado}", null, ct);

    public Task<(int dados, string? erro)> AdicionarPedidoAsync(PedidoCompraPersistenciaRequest dto, CancellationToken ct = default)
        => EnviarAsync<int>(HttpMethod.Post, "/api/compras/pedidos", dto, ct);

    public Task<string?> DeletarPedidoAsync(int id, CancellationToken ct = default)
        => EnviarAsync(HttpMethod.Delete, $"/api/compras/pedidos/{id}", null, ct);

    // ── Família ───────────────────────────────────────────────────────────
    public Task<(List<MembroFamiliaDetalheDTO>? dados, string? erro)> MembrosAsync(CancellationToken ct = default)
        => GetAsync<List<MembroFamiliaDetalheDTO>>("/api/familia/membros", ct);

    public Task<(ResumoFamiliaDTO? dados, string? erro)> ResumoFamiliaAsync(CancellationToken ct = default)
        => GetAsync<ResumoFamiliaDTO>("/api/familia/resumo", ct);

    public Task<string?> AdicionarMembroAsync(NovoMembroRequest dto, CancellationToken ct = default)
        => EnviarAsync(HttpMethod.Post, "/api/familia/membros", dto, ct);

    public Task<string?> AlterarPapelAsync(string usuarioId, string novoPapel, CancellationToken ct = default)
        => EnviarAsync(HttpMethod.Post, $"/api/familia/membros/{usuarioId}/papel", new { novoPapel }, ct);

    public Task<string?> DefinirAtivoMembroAsync(string usuarioId, bool ativo, CancellationToken ct = default)
        => EnviarAsync(HttpMethod.Post, $"/api/familia/membros/{usuarioId}/ativo?ativo={ativo}", null, ct);

    public Task<string?> RemoverMembroAsync(string usuarioId, CancellationToken ct = default)
        => EnviarAsync(HttpMethod.Delete, $"/api/familia/membros/{usuarioId}", null, ct);

    // ── Conta / Configuração / Assinatura ─────────────────────────────────
    public Task<(EmpresaDetalheDTO? dados, string? erro)> ObterContaAsync(CancellationToken ct = default)
        => GetAsync<EmpresaDetalheDTO>("/api/empresa", ct);

    public Task<(ConfiguracaoFamiliaDTO? dados, string? erro)> ObterConfiguracaoAsync(CancellationToken ct = default)
        => GetAsync<ConfiguracaoFamiliaDTO>("/api/configuracao", ct);

    public Task<string?> SalvarConfiguracaoAsync(ConfiguracaoFamiliaDTO dto, CancellationToken ct = default)
        => EnviarAsync(HttpMethod.Put, "/api/configuracao", dto, ct);

    public Task<(AssinaturaAtualDTO? dados, string? erro)> MinhaAssinaturaAsync(CancellationToken ct = default)
        => GetAsync<AssinaturaAtualDTO>("/api/assinatura", ct);

    // ── Feedback ──────────────────────────────────────────────────────────
    public Task<string?> EnviarFeedbackAsync(object dto, CancellationToken ct = default)
        => EnviarAsync(HttpMethod.Post, "/api/feedback", dto, ct);

    // ── Dispositivos (push FCM) ───────────────────────────────────────────
    public Task<string?> RegistrarDispositivoAsync(object dto, CancellationToken ct = default)
        => EnviarAsync(HttpMethod.Post, "/api/dispositivos", dto, ct);
}
