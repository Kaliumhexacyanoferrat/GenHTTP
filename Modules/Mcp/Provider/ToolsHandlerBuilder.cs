using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Mcp.Schema;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Injectors;

using Conversion = GenHTTP.Modules.Conversion;

namespace GenHTTP.Modules.Mcp.Provider;

/// <summary>
/// Configures an endpoint that provides tools to AI agents via the Model Context Protocol.
/// </summary>
public sealed class ToolsHandlerBuilder : IHandlerBuilder<ToolsHandlerBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    private readonly List<MethodToolDefinition> _tools = [];

    private readonly List<string> _origins = [];

    private ServerIdentity? _identity;

    private string? _instructions;

    private TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

    private JsonSerializerOptions? _serialization;

    private IBuilder<InjectionRegistry>? _injectors;

    private Func<Type, IRequest, ValueTask<object>>? _instanceProvider;

    #region Functionality

    /// <summary>
    /// Adds the methods of the given class annotated with <see cref="ToolAttribute" /> as tools.
    /// </summary>
    /// <remarks>
    /// The parameters of a method become the arguments of the tool, except for values
    /// injected by the framework (such as the request or the authenticated user).
    /// Interceptors such as role checks are executed before the method is invoked.
    /// Classes that also provide webservice or controller methods are not supported.
    /// </remarks>
    /// <typeparam name="T">The class providing the tools</typeparam>
    /// <remarks>
    /// The instance the tools are invoked on is created by the configured
    /// <see cref="InstanceProvider" />, or once via its parameterless constructor.
    /// </remarks>
    public ToolsHandlerBuilder Add<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors)] T>() where T : class
    {
        _tools.AddRange(MethodToolDefinition.Discover(typeof(T), null));
        return this;
    }

    /// <summary>
    /// Adds the methods of the given instance annotated with <see cref="ToolAttribute" /> as tools.
    /// </summary>
    /// <param name="instance">The instance providing the tools</param>
    public ToolsHandlerBuilder Add(object instance)
    {
        _tools.AddRange(MethodToolDefinition.Discover(instance.GetType(), _ => new(instance)));
        return this;
    }

    /// <summary>
    /// Adds a tool implemented by the given function.
    /// </summary>
    /// <remarks>
    /// The parameters of the function become the arguments of the tool, except for values
    /// injected by the framework. Attributes on the function (such as role checks or
    /// <see cref="ToolAttribute" /> to specify the title and hints of the tool) and its
    /// parameters (such as descriptions) are respected.
    /// </remarks>
    /// <param name="name">The name of the tool</param>
    /// <param name="description">Describes the purpose of the tool to the model</param>
    /// <param name="function">The function implementing the tool</param>
    public ToolsHandlerBuilder Add(string name, string description, Delegate function)
    {
        _tools.Add(MethodToolDefinition.FromDelegate(name, description, function));
        return this;
    }

    /// <summary>
    /// Adds a concern (such as authentication) to the endpoint.
    /// </summary>
    /// <param name="concern">The concern to be added</param>
    public ToolsHandlerBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    /// <summary>
    /// Sets the logic used to create the instances tools added via <see cref="Add{T}" />
    /// are invoked on, e.g. per request via dependency injection.
    /// </summary>
    /// <remarks>
    /// The provider is only invoked if the arguments of a call are valid and no
    /// interceptor objected.
    /// </remarks>
    /// <param name="provider">Creates the instance of the given type to invoke a tool on</param>
    public ToolsHandlerBuilder InstanceProvider(Func<Type, IRequest, ValueTask<object>> provider)
    {
        _instanceProvider = provider;
        return this;
    }

    /// <summary>
    /// Sets the injectors used to provide framework values (such as the request or
    /// the authenticated user) to tools implemented by methods or functions.
    /// </summary>
    /// <param name="registry">The injectors to be used</param>
    public ToolsHandlerBuilder Injectors(IBuilder<InjectionRegistry> registry)
    {
        _injectors = registry;
        return this;
    }

    /// <summary>
    /// Identifies this server to clients.
    /// </summary>
    /// <param name="name">The name of the server (defaults to the name of the entry assembly)</param>
    /// <param name="version">The version of the server (defaults to the version of the entry assembly)</param>
    /// <param name="title">A human-readable name of the server</param>
    public ToolsHandlerBuilder Server(string name, string version, string? title = null)
    {
        _identity = new ServerIdentity(name, version, title);
        return this;
    }

    /// <summary>
    /// Explains to the model how to use the tools of this server, e.g.
    /// which tool to call first or how tools relate to each other.
    /// </summary>
    /// <remarks>
    /// Clients may shorten long instructions, so keep them focused.
    /// </remarks>
    /// <param name="instructions">The instructions to be passed to the model</param>
    public ToolsHandlerBuilder Instructions(string instructions)
    {
        _instructions = instructions;
        return this;
    }

    /// <summary>
    /// Allows a web page served from the given origin to use this endpoint
    /// from within a browser.
    /// </summary>
    /// <remarks>
    /// Requests from browsers carry an origin and are rejected by default to
    /// prevent malicious web pages from accessing the endpoint (e.g. via DNS
    /// rebinding). Agents do not send an origin and are not affected.
    /// </remarks>
    /// <param name="origin">The origin to allow, such as "https://example.com"</param>
    public ToolsHandlerBuilder AllowOrigin(string origin)
    {
        _origins.Add(origin);
        return this;
    }

    /// <summary>
    /// Sets how long clients may cache the list of tools (defaults to 5 minutes).
    /// </summary>
    /// <param name="duration">The duration clients may consider the tool list fresh</param>
    public ToolsHandlerBuilder CacheDuration(TimeSpan duration)
    {
        if (duration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), "The cache duration must not be negative");
        }

        _cacheDuration = duration;
        return this;
    }

    /// <summary>
    /// Sets the options used to read the arguments and write the results of tools,
    /// and to describe their types to the client.
    /// </summary>
    /// <remarks>
    /// Pass options with a source generated context to avoid reflection.
    /// </remarks>
    /// <param name="options">The serializer options to be used</param>
    public ToolsHandlerBuilder Serialization(JsonSerializerOptions options)
    {
        _serialization = options;
        return this;
    }

    /// <summary>
    /// Creates the endpoint.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if a tool cannot be described to clients</exception>
    public IHandler Build()
    {
        var options = _serialization != null ? McpSerialization.ForRuntime(_serialization) : McpSerialization.Default;
        var schemaOptions = _serialization != null ? McpSerialization.ForSchema(options) : McpSerialization.DefaultSchema;

        var names = new HashSet<string>(StringComparer.Ordinal);

        foreach (var definition in _tools)
        {
            if (!names.Add(definition.Name))
            {
                throw new InvalidOperationException($"There is more than one tool named '{definition.Name}'");
            }
        }

        var instances = new Dictionary<Type, Func<IRequest, ValueTask<object>>>();

        var tools = _tools.Select(t => t.ActivatedType != null ? t with { Instance = GetInstanceProvider(t.ActivatedType, instances) } : t).ToArray();

        var registry = new MethodRegistry(Conversion.Serialization.Default().Build(), (_injectors ?? Injection.Default()).Build(), Conversion.Formatting.Default().Build());

        var handler = new ToolsHandler(tools, registry, _identity ?? ServerIdentity.Default(), _instructions, _cacheDuration, new OriginPolicy(_origins), options, schemaOptions);

        return Concerns.Chain(_concerns, handler);
    }

    private Func<IRequest, ValueTask<object>> GetInstanceProvider(Type type, Dictionary<Type, Func<IRequest, ValueTask<object>>> instances)
    {
        if (instances.TryGetValue(type, out var known))
        {
            return known;
        }

        Func<IRequest, ValueTask<object>> provider;

        var custom = _instanceProvider;

        if (custom != null)
        {
            provider = r => custom(type, r);
        }
        else
        {
            object instance;

            try
            {
                instance = Activator.CreateInstance(type)!;
            }
            catch (MissingMethodException e)
            {
                throw new InvalidOperationException($"Type '{type}' needs a public parameterless constructor to provide tools. Use dependency injection to create instances with dependencies.", e);
            }

            provider = _ => new(instance);
        }

        instances[type] = provider;

        return provider;
    }

    #endregion

}
