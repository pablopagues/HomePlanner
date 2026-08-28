using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.HomePlanner.Services.Identity;

/// <summary>Nomes fixos do provider e do propósito do token de convite.</summary>
public static class ConviteToken
{
    /// <summary>Nome sob o qual o provider é registrado no Identity.</summary>
    public const string Provider = "ConviteMembro";

    /// <summary>Propósito do token — precisa ser o mesmo na geração e na validação.</summary>
    public const string Proposito = "ConviteMembro";

    /// <summary>Validade do convite. Bem maior que a do reset de senha (1 dia, padrão do Identity),
    /// porque um convidado costuma demorar mais para abrir o e-mail do que alguém que acabou de
    /// pedir a redefinição da própria senha.</summary>
    public static readonly TimeSpan Validade = TimeSpan.FromDays(7);

    public static int DiasValidade => (int)Validade.TotalDays;
}

public class ConviteTokenProviderOptions : DataProtectionTokenProviderOptions
{
    public ConviteTokenProviderOptions()
    {
        Name = ConviteToken.Provider;
        TokenLifespan = ConviteToken.Validade;
    }
}

/// <summary>
/// Provider de tokens dedicado ao convite de membros, com validade própria.
/// Mantém o provider padrão (e a validade curta) intacto para o fluxo de "esqueci minha senha".
/// </summary>
public class ConviteTokenProvider<TUser> : DataProtectorTokenProvider<TUser> where TUser : class
{
    public ConviteTokenProvider(
        IDataProtectionProvider dataProtectionProvider,
        IOptions<ConviteTokenProviderOptions> options,
        ILogger<DataProtectorTokenProvider<TUser>> logger)
        : base(dataProtectionProvider, options, logger) { }
}
