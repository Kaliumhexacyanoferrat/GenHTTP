using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Shared.Types.Body;

namespace GenHTTP.Engine.Shared.Infrastructure.Compliance;

public sealed class ComplianceConcern(IHandler content) : IConcern
{

    public IHandler Content => content;

    public ValueTask PrepareAsync(IServer server) => content.PrepareAsync(server);

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var header = request.Header;

        var host = header.Headers.GetEntry(KnownHeaders.Host);

        if (host == null)
        {
            throw new ProviderException(ResponseStatus.BadRequest, "Host header is missing from the request");
        }

        if (host.Value.Bytes.IsEmpty)
        {
            throw new ProviderException(ResponseStatus.BadRequest, "Host header is empty");
        }

        if (header.Method == RequestMethod.Get || header.Method == RequestMethod.Head)
        {
            if (header.Headers.ContainsKey(KnownHeaders.ContentLength) || header.Headers.ContainsKey(KnownHeaders.TransferEncoding))
            {
                throw new ProviderException(ResponseStatus.BadRequest, "GET/HEAD requests must not contain a body");
            }
        }

        if (request.HasBody)
        {
            CheckExpectation(header);
        }

        return content.HandleAsync(request);
    }

    /// <summary>
    /// Rejects any expectation other than 100-continue, which is the only one
    /// defined by HTTP (RFC 9110, section 10.1.1).
    /// </summary>
    /// <remarks>
    /// Requests without a body are not checked, as there is nothing for the client
    /// to wait for. The Expect header of HTTP/1.0 requests is to be ignored.
    /// </remarks>
    private static void CheckExpectation(IRequestHeader header)
    {
        if (header.Protocol == HttpProtocol.Http10)
        {
            return;
        }

        var expectation = header.Headers.GetEntry(KnownHeaders.Expect);

        if (expectation != null && !ContinueReader.IsContinue(expectation.Value))
        {
            throw new ProviderException(ResponseStatus.ExpectationFailed, "The only expectation supported is '100-continue'");
        }
    }

}
