using System.Reflection;
using GenHTTP.Modules.Functional.CodeGen.Model;

namespace GenHTTP.Modules.Functional.CodeGen;

public static class EntryPoint
{

    public static string Emit(GeneratedSource source)
    {
        using var template = Assembly.GetExecutingAssembly()
                                     .GetManifestResourceStream("GenHTTP.Modules.Functional.CodeGen.Templates.EntryPoint.scriban");

        if (template == null)
        {
            throw new InvalidOperationException("Entry point template not found in assembly.");
        }
        
        using var reader = new StreamReader(template);
        
        var parsed = Scriban.Template.Parse(reader.ReadToEnd());

        if (parsed == null)
        {
            throw new InvalidOperationException("Failed to parse entry point template.");
        }

        if (parsed.HasErrors)
        {
            throw new InvalidOperationException($"Failed to parse entry point template: {parsed.Messages}");
        }

        return parsed.Render(source);
    }
    
}
