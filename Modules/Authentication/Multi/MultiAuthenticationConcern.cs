using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Content.Services;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Authentication.Multi;

public sealed class MultiAuthenticationConcern : IDocumentedConcern
{

    #region Get-/Setters

    public IHandler Content { get; }

    private readonly IConcern[] _delegatingConcerns;

    #endregion

    #region Initialization

    public MultiAuthenticationConcern(IHandler content, IConcern[] delegatingConcerns)
    {
        Content = content;
        _delegatingConcerns = delegatingConcerns;
    }

    #endregion

    #region Functionality

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        ResponseOrException? lastResponse = null;

        foreach (var concern in _delegatingConcerns)
        {
            try
            {
                lastResponse = new(await concern.HandleAsync(request));
            }
            catch (ProviderException e)
            {
                lastResponse = new(Exception: e);
            }

            if (lastResponse.Status != ResponseStatus.Unauthorized)
            {
                return lastResponse.Get();
            }
        }

        return lastResponse?.Get() ??
                     request.Respond()
                            .Status(ResponseStatus.Unauthorized)
                            .Build();
    }

    public ValueTask PrepareAsync(IServer server) => Content.PrepareAsync(server);

    public void AddDocumentation(OperationDocumentation operation)
    {
        var schemes = new List<SecurityScheme>();

        var entries = new List<DocumentationAttribute>();

        foreach (var concern in _delegatingConcerns.OfType<IDocumentedConcern>())
        {
            var child = new OperationDocumentation(operation.Method);

            concern.AddDocumentation(child);

            schemes.AddRange(child.Authentication.SelectMany(r => r.AnyOf));
            entries.AddRange(child.Entries);
        }

        if (schemes.Count > 0)
        {
            operation.RequireAuthentication(schemes.Distinct().ToArray());
        }

        operation.Response(ResponseStatus.Unauthorized, "The request could not be authenticated with any of the supported methods");

        foreach (var entry in entries)
        {
            switch (entry)
            {
                case ResponseAttribute response:
                    operation.Response(response.Status, response.Description, response.Type, response.ContentType);
                    break;
                case ResponseHeaderAttribute header:
                    operation.ResponseHeader(header.Name, header.Description, header.Status, header.Type);
                    break;
                case RequestHeaderAttribute header:
                    operation.RequestHeader(header.Name, header.Description, false, header.Type);
                    break;
            }
        }
    }

    #endregion

    #region Helper structure

    private sealed record ResponseOrException(IResponse? Response = null, ProviderException? Exception = null)
    {

        public IResponse? Get()
        {
            if (Exception != null)
            {
                throw Exception;
            }

            return Response;
        }

        public ResponseStatus? Status => Exception?.Status ?? Response?.Status;

    }

    #endregion

}
