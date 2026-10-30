using System.Reflection;
using System.Text;

using GenHTTP.Api.Content.Documentation;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Modules.Mcp.Provider;

/// <summary>
/// Extends the description of a tool method with its remarks and the
/// documentation of its interceptors, so the model knows about the reasons
/// a call might be refused (e.g. missing roles).
/// </summary>
internal static class ToolDocumentation
{

    internal static string Describe(string description, MethodInfo method, Operation operation)
    {
        var remarks = method.GetCustomAttribute<RemarksAttribute>(true)?.Text;

        var errors = GetErrors(operation);

        if (remarks == null && errors.Count == 0)
        {
            return description;
        }

        var result = new StringBuilder(description);

        if (remarks != null)
        {
            result.Append("\n\n").Append(remarks);
        }

        if (errors.Count > 0)
        {
            result.Append("\n\nPossible errors:");

            foreach (var error in errors)
            {
                result.Append("\n- ").Append(error);
            }
        }

        return result.ToString();
    }

    private static List<string> GetErrors(Operation operation)
    {
        var documentation = new OperationDocumentation(RequestMethod.Post);

        foreach (var interceptor in operation.Interceptors)
        {
            if (interceptor is IDocumentedInterceptor documented)
            {
                documented.AddDocumentation(documentation);
            }
        }

        var errors = new List<string>();

        foreach (var response in documentation.Entries.OfType<ResponseAttribute>())
        {
            if ((int)response.Status >= 400 && !string.IsNullOrWhiteSpace(response.Description) && !errors.Contains(response.Description))
            {
                errors.Add(response.Description);
            }
        }

        return errors;
    }

}
