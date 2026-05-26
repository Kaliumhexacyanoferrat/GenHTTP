using System.Text;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.Security.Cors;

public sealed class CorsPolicyHandler : IConcern
{
    private static readonly ReadOnlyMemory<byte> OriginHeader = "Origin"u8.ToArray();

    private static readonly ReadOnlyMemory<byte> AcaOrigin = "Access-Control-Allow-Origin"u8.ToArray();

    private static readonly ReadOnlyMemory<byte> AcaMethods = "Access-Control-Allow-Methods"u8.ToArray();

    private static readonly ReadOnlyMemory<byte> AcaHeaders = "Access-Control-Allow-Headers"u8.ToArray();

    private static readonly ReadOnlyMemory<byte> AcExposeHeaders = "Access-Control-Expose-Headers"u8.ToArray();

    private static readonly ReadOnlyMemory<byte> AcaCredentials = "Access-Control-Allow-Credentials"u8.ToArray();

    private static readonly ReadOnlyMemory<byte> AcMaxAge = "Access-Control-Max-Age"u8.ToArray();

    private static readonly ReadOnlyMemory<byte> TrueValue = "true"u8.ToArray();

    private static readonly ReadOnlyMemory<byte> VaryHeader = "Vary"u8.ToArray();

    public static readonly ReadOnlyMemory<byte> AllowAny = "*"u8.ToArray();

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

    public ValueTask PrepareAsync() => Content.PrepareAsync();

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

    private static void ConfigureResponse(IResponse response, ReadOnlyMemory<byte> origin, OriginPolicy policy)
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

        var ageBuffer = age[..written].ToArray();

        builder.Header(AcMaxAge, ageBuffer);

        if (!origin.Span.SequenceEqual(AllowAny.Span))
        {
            builder.Header(VaryHeader, OriginHeader);
        }
    }

    private (ReadOnlyMemory<byte> origin, OriginPolicy? policy) GetPolicy(IRequest request)
    {
        var origin = request.Header.Headers.GetEntry(OriginHeader);

        if (origin is not null)
        {
            var originString = Encoding.ASCII.GetString(origin.Value.Span);

            if (AdditionalPolicies.TryGetValue(originString, out var policy))
            {
                return (origin.Value, policy);
            }
        }

        return (origin ?? AllowAny, DefaultPolicy);
    }

    private static ReadOnlyMemory<byte> GetListOrWildcard(List<string>? values)
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

        return buffer;
    }

    private static ReadOnlyMemory<byte> GetListOrWildcard(List<RequestMethod>? values)
    {
        if (values is null) return AllowAny;

        var totalLength = 0;

        for (var i = 0; i < values.Count; i++)
        {
            totalLength += values[i].Value.Length;
            if (i < values.Count - 1) totalLength += 2; // ", "
        }

        var buffer = new byte[totalLength];
        var pos = 0;

        for (var i = 0; i < values.Count; i++)
        {
            var span = values[i].Value.Span;
            span.CopyTo(buffer.AsSpan(pos));

            pos += span.Length;

            if (i < values.Count - 1)
            {
                buffer[pos++] = (byte)',';
                buffer[pos++] = (byte)' ';
            }
        }

        return buffer;
    }

    private static bool HasValue<T>(List<T>? list) => list is null || list.Count > 0;

    #endregion

}
