namespace GenHTTP.Modules.Reflection;

/// <summary>
/// Specifies how the service should be executed by
/// the internal engine.
/// </summary>
public enum ExecutionMode
{
    
    /// <summary>
    /// The server will attempt to use code generation
    /// and fall back to reflection, if codegen is not
    /// available in the current environment.
    /// </summary>
    Auto,
    
    /// <summary>
    /// The server executes methods using reflection.
    /// </summary>
    Reflection
    
}
