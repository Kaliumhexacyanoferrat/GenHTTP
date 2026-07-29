using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Shared.Types;

using Microsoft.AspNetCore.Http.Features;

namespace GenHTTP.Adapters.AspNetCore.Context;

/// <summary>
/// Adapts Kestrel's <see cref="IHttpRequestFeature"/> to the engine agnostic
/// <see cref="IRequestHeader"/> contract.
/// </summary>
/// <remarks>
/// Unlike the Internal/Ioxide engines (which fill this information from a raw,
/// Glyph11-parsed wire buffer), Kestrel already parsed the request for us. Path
/// and query are still read from the raw request target (rather than Kestrel's
/// already-decoded <see cref="IHttpRequestFeature.Path"/>/query collection) to
/// keep the same "raw, decode-on-demand" semantics the other engines expose via
/// <see cref="PathSegment.Decode"/>.
/// </remarks>
internal sealed class RequestHeader : IRequestHeader
{
    private readonly RequestTarget _target = new();

    private readonly KeyValueList _headers = new();

    private readonly KeyValueList _query = new();

    private ByteString _path;

    #region Get-/Setters

    public HttpProtocol Protocol { get; private set; }

    public RequestMethod Method { get; private set; }

    public ByteString Path => _path;

    public IRequestTarget Target => _target;

    public IRequestQuery Query => _query;

    public IRequestHeaders Headers => _headers;

    #endregion

    #region Functionality

    public void Apply(IHttpRequestFeature feature)
    {
        Protocol = new(feature.Protocol);
        Method = new(feature.Method);

        var rawTarget = feature.RawTarget;

        var queryIndex = rawTarget.IndexOf('?');

        _path = new ByteString(queryIndex < 0 ? rawTarget : rawTarget[..queryIndex]);

        _headers.Clear();

        foreach (var header in feature.Headers)
        {
            foreach (var value in header.Value)
            {
                if (value is not null)
                {
                    _headers.Add(new ByteString(header.Key), new ByteString(value));
                }
            }
        }

        _query.Clear();

        if (queryIndex >= 0)
        {
            ParseQuery(rawTarget.AsSpan(queryIndex + 1));
        }

        _target.Apply(_path);
    }

    private void ParseQuery(ReadOnlySpan<char> query)
    {
        while (!query.IsEmpty)
        {
            var ampersand = query.IndexOf('&');

            var pair = ampersand < 0 ? query : query[..ampersand];

            if (!pair.IsEmpty)
            {
                var equals = pair.IndexOf('=');

                if (equals < 0)
                {
                    _query.Add(new ByteString(pair.ToString()), new ByteString(""));
                }
                else
                {
                    _query.Add(new ByteString(pair[..equals].ToString()), new ByteString(pair[(equals + 1)..].ToString()));
                }
            }

            if (ampersand < 0)
            {
                break;
            }

            query = query[(ampersand + 1)..];
        }
    }

    #endregion

}
