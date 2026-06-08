using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.Files.Single;

public sealed class ResourceAssetHandler(IResource resource, bool asDownload, string? fileName, ContentType? contentType) : IHandler
{

    public ValueTask PrepareAsync(IServer server) => default;

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var target = request.Header.Target;

        if (target.Current != null)
        {
            return default;
        }

        if (!request.HasType(RequestMethod.Get, RequestMethod.Head))
        {
            throw new ProviderException(ResponseStatus.MethodNotAllowed, "Only GET requests are allowed by this handler", b => b.Header("Allow", "GET"));
        }

        var response = request.Respond()
                              .Content(new ResourceContent(resource, contentType));

        if (asDownload)
        {
            var name = fileName ?? resource.Name;

            if (name is not null)
            {
                response.Header("Content-Disposition", $"attachment; filename=\"{name}\"");
            }
            else
            {
                response.Header("Content-Disposition", "attachment");
            }
        }

        return new ValueTask<IResponse?>(response.Build());
    }

}
