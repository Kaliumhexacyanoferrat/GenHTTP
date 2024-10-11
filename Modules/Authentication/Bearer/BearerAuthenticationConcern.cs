using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Text.Json.Serialization;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;

namespace GenHTTP.Modules.Authentication.Bearer;

#region Supporting data structures

internal class OpenIDConfiguration
{

    [JsonPropertyName("jwks_uri")]
    public string? KeySetUrl { get; set; }

}

#endregion

internal sealed class BearerAuthenticationConcern : IConcern
{
    private ICollection<SecurityKey>? _IssuerKeys = null;

    #region Get-/Setters

    public IHandler Content { get; }

    public IHandler Parent { get; }

    private TokenValidationOptions ValidationOptions { get; }

    #endregion

    #region Initialization

    internal BearerAuthenticationConcern(IHandler parent, Func<IHandler, IHandler> contentFactory, TokenValidationOptions validationOptions)
    {
            Parent = parent;
            Content = contentFactory(this);

            ValidationOptions = validationOptions;
        }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync() => Content.PrepareAsync();

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
            IdentityModelEventSource.LogCompleteSecurityArtifact = true;

            if (!request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                throw new ProviderException(ResponseStatus.Unauthorized, "This endpoint requires authorization");
            }

            if (!authHeader.StartsWith("Bearer "))
            {
                throw new ProviderException(ResponseStatus.Unauthorized, "This endpoint requires bearer token authentication");
            }

            var tokenString = authHeader[7..];

            var tokenHandler = new JwtSecurityTokenHandler();

            if (!tokenHandler.CanReadToken(tokenString))
            {
                throw new ProviderException(ResponseStatus.BadRequest, "Malformed authentication token");
            }

            var issuer = ValidationOptions.Issuer;

            var audience = ValidationOptions.Audience;

            if ((issuer != null) && (_IssuerKeys == null))
            {
                _IssuerKeys = await FetchSigningKeys(issuer);
            }

            var validationParameters = new TokenValidationParameters()
            {
                ValidIssuer = issuer,
                ValidateIssuer = (issuer != null),
                IssuerSigningKeys = _IssuerKeys,
                ValidAudience = audience,
                ValidateAudience = (audience != null),
                ValidateLifetime = ValidationOptions.Lifetime
            };

            if (issuer == null)
            {
                validationParameters.SignatureValidator = (string token, TokenValidationParameters p) => new JwtSecurityToken(tokenString);
            }

            try
            {
                tokenHandler.ValidateToken(tokenString, validationParameters, out var validated);

                var jwt = (JwtSecurityToken)validated;

                if (ValidationOptions.CustomValidator != null)
                {
                    await ValidationOptions.CustomValidator.Invoke(jwt);
                }

                if (ValidationOptions.UserMapping != null)
                {
                    var user = await ValidationOptions.UserMapping.Invoke(request, jwt);

                    if (user != null)
                    {
                        request.SetUser(user);
                    }
                }

                return await Content.HandleAsync(request);
            }
            catch (SecurityTokenValidationException ex)
            {
                throw new ProviderException(ResponseStatus.Unauthorized, $"Authorization failed: {ex.Message}", ex);
            }
        }

    private static async Task<ICollection<SecurityKey>> FetchSigningKeys(string issuer)
    {
            try
            {
                var configUrl = $"{issuer}/.well-known/openid-configuration";

                using var httpClient = new HttpClient();

                var configResponse = await httpClient.GetStringAsync(configUrl);

                var config = JsonSerializer.Deserialize<OpenIDConfiguration>(configResponse)
                       ?? throw new InvalidOperationException($"Unable to discover configuration via '{configUrl}'");

                var keyResponse = await httpClient.GetStringAsync(config.KeySetUrl);

                var keySet = new JsonWebKeySet(keyResponse);

                return keySet.GetSigningKeys();
            }
            catch (Exception ex)
            {
                throw new ProviderException(ResponseStatus.InternalServerError, "Unable to fetch signing issuer signing keys", ex);
            }
        }

}

#endregion
