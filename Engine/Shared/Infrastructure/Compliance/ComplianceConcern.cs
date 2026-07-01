using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Shared.Infrastructure.Compliance;

public class ComplianceConcern(IHandler content) : IConcern
{

    public IHandler Content => content;

    public ValueTask PrepareAsync(IServer server) => content.PrepareAsync(server);

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var host = request.Header.Headers.GetEntry(KnownHeaders.Host);

        if (host == null)
        {
            throw new ProviderException(ResponseStatus.BadRequest, "Host header is missing from the request");
        }

        if (host.Value.Bytes.IsEmpty)
        {
            throw new ProviderException(ResponseStatus.BadRequest, "Host header is empty");
        }

        return content.HandleAsync(request);
    }

}
