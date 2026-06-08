using System.Text;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.Security.Cors;

public sealed class CorsPolicyHandler : IConcern
{
    private static readonly ByteString OriginHeader = new("Origin");

    private static readonly ByteString AcaOrigin = new("Access-Control-Allow-Origin");

    private static readonly ByteString AcaMethods = new("Access-Control-Allow-Methods");

    private static readonly ByteString AcaHeaders = new("Access-Control-Allow-Headers");

    private static readonly ByteString AcExposeHeaders = new("Access-Control-Expose-Headers");

    private static readonly ByteString AcaCredentials = new("Access-Control-Allow-Credentials");

    private static readonly ByteString AcMaxAge = new("Access-Control-Max-Age");

    private static readonly ByteString TrueValue = new("true");

    private static readonly ByteString VaryHeader = new("Vary");

    public static readonly ByteString AllowAny = new("*");

    #region Get-/Setters

    public IHandler Content { get; }

    public OriginPolicy? DefaultPolicy { get; }

    public IDictionary<string, OriginPolicy?> AdditionalPolicies { get; }

    #endregion

    #region Initialization

    public CorsPolicyHandler(IHandler content, OriginPolicy? defaultPolicy, IDictionary<string, OriginPolicy?> additionalPolicies)
    {
        Content = content;

        DefaultPolicy = defaultPolicy;
        AdditionalPolicies = additionalPolicies;
    }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync(IServer server) => Content.PrepareAsync(server);

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var (origin, policy) = GetPolicy(request);

        IResponse? response;

        if (request.HasType(RequestMethod.Options))
        {
            response = request.Respond()
                              .Status(ResponseStatus.NoContent)
                              .Build();
        }
        else
        {
            response = await Content.HandleAsync(request);
        }

        if (response is not null && policy is not null)
        {
            ConfigureResponse(response, origin, policy);
        }

        return response;
    }

    private static void ConfigureResponse(IResponse response, ByteString origin, OriginPolicy policy)
    {
        var builder = response.Rebuild();

        builder.Header(AcaOrigin, origin);

        if (HasValue(policy.AllowedMethods))
        {
            builder.Header(AcaMethods, GetListOrWildcard(policy.AllowedMethods));
        }

        if (HasValue(policy.AllowedHeaders))
        {
            builder.Header(AcaHeaders, GetListOrWildcard(policy.AllowedHeaders));
        }

        if (HasValue(policy.ExposedHeaders))
        {
            builder.Header(AcExposeHeaders, GetListOrWildcard(policy.ExposedHeaders));
        }

        if (policy.AllowCredentials)
        {
            builder.Header(AcaCredentials, TrueValue);
        }

        Span<byte> age = stackalloc byte[20];

        policy.MaxAge.TryFormat(age, out var written);

        var ageBuffer = new ByteString(age[..written].ToArray());

        builder.Header(AcMaxAge, ageBuffer);

        if (origin != AllowAny)
        {
            builder.Header(VaryHeader, OriginHeader);
        }
    }

    private (ByteString origin, OriginPolicy? policy) GetPolicy(IRequest request)
    {
        var origin = request.Header.Headers.GetEntry(OriginHeader);

        if (origin is not null)
        {
            var originString = origin.Value.ToString();

            if (AdditionalPolicies.TryGetValue(originString, out var policy))
            {
                return (origin.Value, policy);
            }
        }

        return (origin ?? AllowAny, DefaultPolicy);
    }

    private static ByteString GetListOrWildcard(List<string>? values)
    {
        if (values is null) return AllowAny;

        var totalLength = 0;

        for (var i = 0; i < values.Count; i++)
        {
            totalLength += values[i].Length;
            if (i < values.Count - 1) totalLength += 2; // ", "
        }

        var buffer = new byte[totalLength];
        var pos = 0;

        for (var i = 0; i < values.Count; i++)
        {
            pos += Encoding.ASCII.GetBytes(values[i], buffer.AsSpan(pos));

            if (i < values.Count - 1)
            {
                buffer[pos++] = (byte)',';
                buffer[pos++] = (byte)' ';
            }
        }

        return new(buffer);
    }

    private static ByteString GetListOrWildcard(List<RequestMethod>? values)
    {
        if (values is null) return AllowAny;

        var totalLength = 0;

        for (var i = 0; i < values.Count; i++)
        {
            totalLength += values[i].Bytes.Length;
            if (i < values.Count - 1) totalLength += 2; // ", "
        }

        var buffer = new byte[totalLength];
        var pos = 0;

        for (var i = 0; i < values.Count; i++)
        {
            var span = values[i].Bytes.Span;
            span.CopyTo(buffer.AsSpan(pos));

            pos += span.Length;

            if (i < values.Count - 1)
            {
                buffer[pos++] = (byte)',';
                buffer[pos++] = (byte)' ';
            }
        }

        return new(buffer);
    }

    private static bool HasValue<T>(List<T>? list) => list is null || list.Count > 0;

    #endregion

}
