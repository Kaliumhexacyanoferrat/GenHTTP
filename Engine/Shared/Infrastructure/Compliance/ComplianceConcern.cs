using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

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

        return content.HandleAsync(request);
    }

}
