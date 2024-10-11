using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Conversion.Formatters;
using GenHTTP.Modules.Conversion.Serializers;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Injectors;

namespace GenHTTP.Modules.Functional.Provider;

public class InlineBuilder : IHandlerBuilder<InlineBuilder>
{
    private static readonly HashSet<FlexibleRequestMethod> ALL_METHODS = new(Enum.GetValues<RequestMethod>().Select(m => FlexibleRequestMethod.Get(m)));

    private readonly List<IConcernBuilder> _Concerns = new();

    private readonly List<InlineFunction> _Functions = new();

    private IBuilder<FormatterRegistry>? _Formatters;

    private IBuilder<InjectionRegistry>? _Injectors;

    private IBuilder<SerializationRegistry>? _Serializers;

    #region Functionality

    /// <summary>
    /// Configures the serialization registry to be used by this handler. Allows
    /// to add support for additional formats such as protobuf.
    /// </summary>
    /// <param name="registry">The registry to be used by the handler</param>
    public InlineBuilder Serializers(IBuilder<SerializationRegistry> registry)
    {
        _Serializers = registry;
        return this;
    }

    /// <summary>
    /// Configures the injectors to be used to extract complex parameter values.
    /// </summary>
    /// <param name="registry">The registry to be used by the handler</param>
    public InlineBuilder Injectors(IBuilder<InjectionRegistry> registry)
    {
        _Injectors = registry;
        return this;
    }

    /// <summary>
    /// Configures the formatters to be used to extract path values.
    /// </summary>
    /// <param name="registry">The registry to be used by the handler</param>
    public InlineBuilder Formatters(IBuilder<FormatterRegistry> registry)
    {
        _Formatters = registry;
        return this;
    }

    /// <summary>
    /// Adds a route for a request of any type to the root of the handler.
    /// </summary>
    /// <param name="function">The logic to be executed</param>
    /// <param name="ignoreContent">True to exclude the content from sitemaps etc.</param>
    /// <param name="contentHints">A type implementing IContentHints to allow content discovery</param>
    public InlineBuilder Any(Delegate function, bool ignoreContent = false, Type? contentHints = null) => On(function, ALL_METHODS, null, ignoreContent, contentHints);

    /// <summary>
    /// Adds a route for a request of any type to the specified path.
    /// </summary>
    /// <param name="path">The path of the request to handle (e.g. "/my-method")</param>
    /// <param name="function">The logic to be executed</param>
    /// <param name="ignoreContent">True to exclude the content from sitemaps etc.</param>
    /// <param name="contentHints">A type implementing IContentHints to allow content discovery</param>
    public InlineBuilder Any(string path, Delegate function, bool ignoreContent = false, Type? contentHints = null) => On(function, ALL_METHODS, path, ignoreContent, contentHints);

    /// <summary>
    /// Adds a route for a GET request to the root of the handler.
    /// </summary>
    /// <param name="function">The logic to be executed</param>
    /// <param name="ignoreContent">True to exclude the content from sitemaps etc.</param>
    /// <param name="contentHints">A type implementing IContentHints to allow content discovery</param>
    public InlineBuilder Get(Delegate function, bool ignoreContent = false, Type? contentHints = null) => On(function, new HashSet<FlexibleRequestMethod>
                                                                                                                 { FlexibleRequestMethod.Get(RequestMethod.Get) }, null, ignoreContent, contentHints);

    /// <summary>
    /// Adds a route for a GET request to the specified path.
    /// </summary>
    /// <param name="path">The path of the request to handle (e.g. "/my-method")</param>
    /// <param name="function">The logic to be executed</param>
    /// <param name="ignoreContent">True to exclude the content from sitemaps etc.</param>
    /// <param name="contentHints">A type implementing IContentHints to allow content discovery</param>
    public InlineBuilder Get(string path, Delegate function, bool ignoreContent = false, Type? contentHints = null) => On(function, new HashSet<FlexibleRequestMethod>
                                                                                                                              { FlexibleRequestMethod.Get(RequestMethod.Get) }, path, ignoreContent, contentHints);

    /// <summary>
    /// Adds a route for a HEAD request to the root of the handler.
    /// </summary>
    /// <param name="function">The logic to be executed</param>
    public InlineBuilder Head(Delegate function) => On(function, new HashSet<FlexibleRequestMethod>
                                                           { FlexibleRequestMethod.Get(RequestMethod.Head) });

    /// <summary>
    /// Adds a route for a HEAD request to the specified path.
    /// </summary>
    /// <param name="path">The path of the request to handle (e.g. "/my-method")</param>
    public InlineBuilder Head(string path, Delegate function) => On(function, new HashSet<FlexibleRequestMethod>
                                                                        { FlexibleRequestMethod.Get(RequestMethod.Head) }, path);

    /// <summary>
    /// Adds a route for a POST request to the root of the handler.
    /// </summary>
    /// <param name="function">The logic to be executed</param>
    public InlineBuilder Post(Delegate function) => On(function, new HashSet<FlexibleRequestMethod>
                                                           { FlexibleRequestMethod.Get(RequestMethod.Post) });

    /// <summary>
    /// Adds a route for a POST request to the specified path.
    /// </summary>
    /// <param name="path">The path of the request to handle (e.g. "/my-method")</param>
    public InlineBuilder Post(string path, Delegate function) => On(function, new HashSet<FlexibleRequestMethod>
                                                                        { FlexibleRequestMethod.Get(RequestMethod.Post) }, path);

    /// <summary>
    /// Adds a route for a PUT request to the root of the handler.
    /// </summary>
    /// <param name="function">The logic to be executed</param>
    public InlineBuilder Put(Delegate function) => On(function, new HashSet<FlexibleRequestMethod>
                                                          { FlexibleRequestMethod.Get(RequestMethod.Put) });

    /// <summary>
    /// Adds a route for a PUT request to the specified path.
    /// </summary>
    /// <param name="path">The path of the request to handle (e.g. "/my-method")</param>
    public InlineBuilder Put(string path, Delegate function) => On(function, new HashSet<FlexibleRequestMethod>
                                                                       { FlexibleRequestMethod.Get(RequestMethod.Put) }, path);

    /// <summary>
    /// Adds a route for a DELETE request to the root of the handler.
    /// </summary>
    /// <param name="function">The logic to be executed</param>
    public InlineBuilder Delete(Delegate function) => On(function, new HashSet<FlexibleRequestMethod>
                                                             { FlexibleRequestMethod.Get(RequestMethod.Delete) });

    /// <summary>
    /// Adds a route for a DELETE request to the specified path.
    /// </summary>
    /// <param name="path">The path of the request to handle (e.g. "/my-method")</param>
    public InlineBuilder Delete(string path, Delegate function) => On(function, new HashSet<FlexibleRequestMethod>
                                                                          { FlexibleRequestMethod.Get(RequestMethod.Delete) }, path);

    /// <summary>
    /// Executes the given function for the specified path and method.
    /// </summary>
    /// <param name="function">The logic to be executed</param>
    /// <param name="methods">The HTTP verbs to respond to</param>
    /// <param name="path">The path which needs to be specified by the client to call this logic</param>
    /// <param name="ignoreContent">True to exclude the content from sitemaps etc.</param>
    /// <param name="contentHints">A type implementing IContentHints to allow content discovery</param>
    public InlineBuilder On(Delegate function, HashSet<FlexibleRequestMethod>? methods = null, string? path = null, bool ignoreContent = false, Type? contentHints = null)
    {
        var requestMethods = methods ?? new HashSet<FlexibleRequestMethod>();

        if (requestMethods.Count == 1 && requestMethods.Contains(FlexibleRequestMethod.Get(RequestMethod.Get)))
        {
            requestMethods.Add(FlexibleRequestMethod.Get(RequestMethod.Head));
        }

        if (path?.StartsWith('/') == true)
        {
            path = path[1..];
        }

        var config = new MethodConfiguration(requestMethods, ignoreContent, contentHints);

        _Functions.Add(new InlineFunction(path, config, function));

        return this;
    }

    public InlineBuilder Add(IConcernBuilder concern)
    {
        _Concerns.Add(concern);
        return this;
    }

    public IHandler Build(IHandler parent)
    {
        var serializers = (_Serializers ?? Serialization.Default()).Build();

        var injectors = (_Injectors ?? Injection.Default()).Build();

        var formatters = (_Formatters ?? Formatting.Default()).Build();

        return Concerns.Chain(parent, _Concerns, p => new InlineHandler(p, _Functions, serializers, injectors, formatters));
    }

    #endregion

}
