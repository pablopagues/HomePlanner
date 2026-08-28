namespace Application.HomePlanner.Common;

public class ResultadoOperacao
{
    public bool Sucesso { get; init; }

    /// <summary>
    /// Erros com código, para o cliente traduzir. <c>ToString()</c> de cada um devolve o
    /// texto padrão, então <c>string.Join(", ", Erros)</c> continua funcionando onde a
    /// tradução ainda não foi ligada.
    /// </summary>
    public IReadOnlyList<ErroOperacao> Erros { get; init; } = [];

    public static ResultadoOperacao Ok() => new() { Sucesso = true };

    /// <summary>Falha com código — a forma preferida. Use as constantes de <see cref="ErrosApp"/>.</summary>
    public static ResultadoOperacao Falha(params ErroOperacao[] erros)
        => new() { Sucesso = false, Erros = erros };

    /// <summary>
    /// Falha com texto solto. Mantida para as mensagens ainda não migradas e para o que
    /// vem pronto de fora (Identity, Stripe) — esses erros passam como "externos" e o
    /// cliente exibe o texto como veio.
    /// </summary>
    public static ResultadoOperacao Falha(params string[] erros)
        => new() { Sucesso = false, Erros = erros.Select(ErroOperacao.Externa).ToArray() };
}

public class ResultadoOperacao<T>
{
    public bool Sucesso { get; init; }
    public T? Valor { get; init; }
    public IReadOnlyList<ErroOperacao> Erros { get; init; } = [];

    public static ResultadoOperacao<T> Ok(T valor) => new() { Sucesso = true, Valor = valor };

    public static ResultadoOperacao<T> Falha(params ErroOperacao[] erros)
        => new() { Sucesso = false, Erros = erros };

    public static ResultadoOperacao<T> Falha(params string[] erros)
        => new() { Sucesso = false, Erros = erros.Select(ErroOperacao.Externa).ToArray() };
}

public class ResultadoListagem<T>
{
    public IReadOnlyList<T> Itens { get; init; } = [];
    public int Total { get; init; }
    public int Pagina { get; init; }
    public int TamanhoPagina { get; init; }
    public int TotalPaginas => TamanhoPagina > 0 ? (int)Math.Ceiling((double)Total / TamanhoPagina) : 0;
}
