namespace GenHTTP.Modules.Mcp;

/// <summary>
/// Marks a method of a class registered via <see cref="Provider.ToolsHandlerBuilder.Add{T}()" />
/// as a tool that can be invoked by an AI agent.
/// </summary>
/// <example>
/// On a function, the attribute provides the title and the hints of the tool:
/// <code>
/// Tools.Create().Add("get_time", "Returns the current time", [Tool(ReadOnly = true)] () => DateTime.UtcNow);
/// </code>
/// </example>
/// <remarks>
/// The tool is described by the <see cref="GenHTTP.Api.Content.Documentation.SummaryAttribute" /> or
/// the <see cref="System.ComponentModel.DescriptionAttribute" /> on the method. Its parameters become
/// the arguments of the tool, except for the ones that are injected by the framework (such as the request
/// or the authenticated user). Interceptors (such as role checks) are executed before the method is invoked.
/// </remarks>
/// <param name="name">The name of the tool (defaults to the name of the method in snake case, e.g. "create_user" for "CreateUserAsync")</param>
[AttributeUsage(AttributeTargets.Method)]
public sealed class ToolAttribute(string? name = null) : Attribute
{
    private bool? _readOnly, _destructive, _idempotent, _openWorld;

    #region Get-/Setters

    /// <summary>
    /// The name of the tool, if specified.
    /// </summary>
    public string? Name => name;

    /// <summary>
    /// A human-readable name of the tool, shown by clients.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// The tool does not modify its environment.
    /// </summary>
    public bool ReadOnly { get => _readOnly ?? false; set => _readOnly = value; }

    /// <summary>
    /// The tool may perform destructive updates.
    /// </summary>
    public bool Destructive { get => _destructive ?? false; set => _destructive = value; }

    /// <summary>
    /// Calling the tool repeatedly with the same arguments has no additional effect.
    /// </summary>
    public bool Idempotent { get => _idempotent ?? false; set => _idempotent = value; }

    /// <summary>
    /// The tool interacts with external entities (such as the web).
    /// </summary>
    public bool OpenWorld { get => _openWorld ?? false; set => _openWorld = value; }

    internal Provider.ToolHints? Hints
        => (_readOnly ?? _destructive ?? _idempotent ?? _openWorld) == null
               ? null
               : new Provider.ToolHints(_readOnly, _destructive, _idempotent, _openWorld);

    #endregion

}
