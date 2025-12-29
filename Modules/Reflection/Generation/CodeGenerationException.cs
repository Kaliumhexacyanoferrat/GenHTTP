namespace GenHTTP.Modules.Reflection.Generation;

public class CodeGenerationException(string? code, Exception inner) : Exception("Failed to compile code for generated handler", inner)
{

    public string? Code => code;

}
