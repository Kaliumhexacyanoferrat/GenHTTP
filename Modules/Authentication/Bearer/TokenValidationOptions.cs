using System.IdentityModel.Tokens.Jwt;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Authentication.Bearer;

internal sealed class TokenValidationOptions
{

    internal string? Audience { get; set; }

    internal string? Issuer { get; set; }

    internal bool Lifetime { get; set; } = true;

    internal Func<JwtSecurityToken, Task>? CustomValidator { get; set; }

    internal Func<IRequest, JwtSecurityToken, ValueTask<IUser?>>? UserMapping { get; set; }

}