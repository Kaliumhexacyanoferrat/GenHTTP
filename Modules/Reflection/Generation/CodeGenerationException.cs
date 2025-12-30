namespace GenHTTP.Modules.Reflection.Generation;

/// <summary>
/// Thrown if the server failed to compile generated code into a delegate.
/// </summary>
/// <param name="code">The code that caused the issue</param>
/// <param name="inner">Information about the actual cause</param>
public class CodeGenerationException(string? code, Exception inner) : Exception("Failed to compile code for generated handler", inner)
{

    /// <summary>
    /// The code that caused the issue.
    /// </summary>
    public string? Code => code;

}
