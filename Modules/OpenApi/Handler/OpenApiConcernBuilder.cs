using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Content.Services;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.OpenApi.Discovery;
using NSwag;

namespace GenHTTP.Modules.OpenApi.Handler;

public sealed class OpenApiConcernBuilder : IConcernBuilder
{
    private readonly List<AuthenticationRequirement> _security = [];

    private bool _caching = true;

    private Action<IRequest, OpenApiDocument>? _postProcessor;

    private string? _title, _version;

    #region Initialization

    public OpenApiConcernBuilder(ApiDiscoveryRegistry registry)
    {
        Discovery = registry;
    }

    #endregion

    #region Get-/Setters

    private ApiDiscoveryRegistry Discovery { get; }

    #endregion

    #region Functionality

    /// <summary>
    /// Sets the title of the OpenAPI specification.
    /// </summary>
    /// <param name="title">The title of the API</param>
    public OpenApiConcernBuilder Title(string title)
    {
        _title = title;
        return this;
    }

    /// <summary>
    /// Sets the version of the described API.
    /// </summary>
    /// <param name="version">The version of the API</param>
    public OpenApiConcernBuilder Version(string version)
    {
        _version = version;
        return this;
    }

    /// <summary>
    /// Specifies, whether the generated OpenAPI specification should
    /// get cached on first request, so it is no re-generated on every request.
    /// </summary>
    /// <param name="enabled">Whether to use caching or not</param>
    public OpenApiConcernBuilder Caching(bool enabled)
    {
        _caching = enabled;
        return this;
    }

    /// <summary>
    /// Declares that the client needs to authenticate with any one of the given
    /// schemes to access the operations of the API.
    /// </summary>
    /// <param name="anyOf">The schemes the client may choose from</param>
    /// <remarks>
    /// Use this method to document authentication that cannot be discovered from
    /// the handler chain (e.g. because it is performed by an API gateway or because
    /// the specification itself is served behind an authentication concern). This
    /// does not enforce authentication. If called multiple times, all of the
    /// requirements need to be satisfied. Operations secured by a concern
    /// need to satisfy both the requirements declared here and by the concern.
    /// </remarks>
    public OpenApiConcernBuilder Security(params SecurityScheme[] anyOf)
    {
        if (anyOf.Length == 0)
        {
            throw new ArgumentException("At least one security scheme is required", nameof(anyOf));
        }

        _security.Add(new AuthenticationRequirement(anyOf));
        return this;
    }

    /// <summary>
    /// Registers a function that will be called when an OpenAPI document has been
    /// generated, directly before it is served to the client.
    /// </summary>
    /// <param name="action">The method to be invoked to adjust the generated document</param>
    public OpenApiConcernBuilder PostProcessor(Action<IRequest, OpenApiDocument> action)
    {
        _postProcessor = action;
        return this;
    }

    public IConcern Build(IHandler content) => new OpenApiConcern(content, Discovery, _caching, DoPostProcessing);

    private void DoPostProcessing(IRequest request, OpenApiDocument document)
    {
        if (_security.Count > 0)
        {
            ApplySecurity(document);
        }

        if (_title != null)
        {
            document.Info.Title = _title;
        }

        if (_version != null)
        {
            document.Info.Version = _version;
        }

        _postProcessor?.Invoke(request, document);
    }

    private void ApplySecurity(OpenApiDocument document)
    {
        var global = SecurityDocumentation.GetRequirements(document, _security);

        document.Security = global;

        // the security of an operation replaces the global one, so both need to be stated
        foreach (var operation in document.Paths.Values.SelectMany(p => p.Values))
        {
            if (operation.Security?.Count > 0)
            {
                operation.Security = SecurityDocumentation.Combine(global, operation.Security);
            }
        }
    }

    #endregion

}
