namespace Application.HomePlanner.Common;

/// <summary>
/// Erro devolvido por um serviço.
///
/// O que atravessa a fronteira é o <see cref="Codigo"/>, não texto: quem exibe decide o
/// idioma. A API serializa o código, e web e app procuram a chave <c>Erro_{Codigo}</c>
/// no recurso compartilhado.
///
/// <see cref="TextoPadrao"/> existe como rede de segurança: se a chave ainda não foi
/// traduzida, o cliente cai nele em vez de mostrar o código cru na tela. É isso que
/// permite traduzir os erros aos poucos sem quebrar nada.
/// </summary>
public sealed class ErroOperacao
{
    /// <summary>Código estável — vira a chave <c>Erro_{Codigo}</c> no recurso.</summary>
    public string Codigo { get; }

    /// <summary>Texto em português usado quando não há tradução para o código.</summary>
    public string TextoPadrao { get; }

    /// <summary>Valores que preenchem os <c>{0}</c>, <c>{1}</c>… da mensagem.</summary>
    public IReadOnlyList<object?> Args { get; }

    /// <summary>
    /// Verdadeiro quando o texto veio pronto de fora (ASP.NET Identity, Stripe) e não
    /// tem código nosso. Nesse caso o cliente exibe <see cref="TextoPadrao"/> direto.
    /// </summary>
    public bool Externo { get; }

    private ErroOperacao(string codigo, string textoPadrao, object?[] args, bool externo)
    {
        Codigo = codigo;
        TextoPadrao = textoPadrao;
        Args = args;
        Externo = externo;
    }

    /// <summary>Erro nosso, com código e texto padrão. Use as constantes de <c>ErrosApp</c>.</summary>
    public static ErroOperacao De(string codigo, string textoPadrao, params object?[] args)
        => new(codigo, textoPadrao, args, externo: false);

    /// <summary>Mensagem gerada por biblioteca de terceiros — passa direto, sem tradução nossa.</summary>
    public static ErroOperacao Externa(string texto)
        => new("externo", texto, [], externo: true);

    public override string ToString() => TextoPadrao;

    /// <summary>
    /// Converte para o texto padrão, como <c>LocalizedString</c> faz. Existe para os
    /// pontos que ainda não traduzem — sem isso, ligar os códigos exigiria migrar todos
    /// os consumidores no mesmo commit.
    /// </summary>
    public static implicit operator string(ErroOperacao erro) => erro.TextoPadrao;
}
