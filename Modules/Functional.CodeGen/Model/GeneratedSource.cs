namespace GenHTTP.Modules.Functional.CodeGen.Model;

public class GeneratedSource
{
    
    public string Generator { get; }
    
    public string GeneratorVersion { get; }
    
    public List<GeneratedHandler> Handlers { get; }

    public GeneratedSource(string generator, string generatorVersion, List<GeneratedHandler> handlers)
    {
        Generator = generator;
        GeneratorVersion = generatorVersion;
        Handlers = handlers;
    }
    
}
