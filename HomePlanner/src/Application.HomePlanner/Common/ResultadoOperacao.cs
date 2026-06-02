namespace Application.HomePlanner.Common;

public class ResultadoOperacao
{
    public bool Sucesso { get; init; }
    public IReadOnlyList<string> Erros { get; init; } = [];

    public static ResultadoOperacao Ok() => new() { Sucesso = true };
    public static ResultadoOperacao Falha(params string[] erros) => new() { Sucesso = false, Erros = erros };
}

public class ResultadoOperacao<T>
{
    public bool Sucesso { get; init; }
    public T? Valor { get; init; }
    public IReadOnlyList<string> Erros { get; init; } = [];

    public static ResultadoOperacao<T> Ok(T valor) => new() { Sucesso = true, Valor = valor };
    public static ResultadoOperacao<T> Falha(params string[] erros) => new() { Sucesso = false, Erros = erros };
}

public class ResultadoListagem<T>
{
    public IReadOnlyList<T> Itens { get; init; } = [];
    public int Total { get; init; }
    public int Pagina { get; init; }
    public int TamanhoPagina { get; init; }
    public int TotalPaginas => TamanhoPagina > 0 ? (int)Math.Ceiling((double)Total / TamanhoPagina) : 0;
}
