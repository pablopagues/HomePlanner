using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Resources.HomePlanner;

namespace Infrastructure.HomePlanner.Services.Identity;

/// <summary>
/// Mensagens do ASP.NET Identity (política de senha, e-mail duplicado, token inválido...)
/// no idioma da requisição, via resx compartilhado (chaves <c>Identity_*</c>). Vale para
/// registro, redefinição e troca de senha — sem tradução manual por código de erro.
/// </summary>
public class IdentityErrorDescriberLocalizado : IdentityErrorDescriber
{
    private readonly IStringLocalizer<SharedResource> _l;

    public IdentityErrorDescriberLocalizado(IStringLocalizer<SharedResource> l) => _l = l;

    private IdentityError Erro(string codigo, params object[] args) => new()
    {
        Code = codigo,
        Description = args.Length == 0 ? _l[$"Identity_{codigo}"] : _l[$"Identity_{codigo}", args],
    };

    public override IdentityError DefaultError() => Erro(nameof(DefaultError));
    public override IdentityError PasswordMismatch() => Erro(nameof(PasswordMismatch));
    public override IdentityError InvalidToken() => Erro(nameof(InvalidToken));
    public override IdentityError RecoveryCodeRedemptionFailed() => Erro(nameof(RecoveryCodeRedemptionFailed));
    public override IdentityError InvalidEmail(string? email) => Erro(nameof(InvalidEmail), email ?? string.Empty);
    public override IdentityError DuplicateEmail(string email) => Erro(nameof(DuplicateEmail), email);
    public override IdentityError DuplicateUserName(string userName) => Erro(nameof(DuplicateUserName), userName);
    public override IdentityError UserAlreadyHasPassword() => Erro(nameof(UserAlreadyHasPassword));
    public override IdentityError PasswordTooShort(int length) => Erro(nameof(PasswordTooShort), length);
    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => Erro(nameof(PasswordRequiresUniqueChars), uniqueChars);
    public override IdentityError PasswordRequiresNonAlphanumeric() => Erro(nameof(PasswordRequiresNonAlphanumeric));
    public override IdentityError PasswordRequiresDigit() => Erro(nameof(PasswordRequiresDigit));
    public override IdentityError PasswordRequiresLower() => Erro(nameof(PasswordRequiresLower));
    public override IdentityError PasswordRequiresUpper() => Erro(nameof(PasswordRequiresUpper));
}
