using System.Reflection;

using GenHTTP.Api.Content.Services;

namespace GenHTTP.Modules.OpenApi.Discovery;

/// <summary>
/// Reads the documentation attributes (such as <see cref="SummaryAttribute" />)
/// declared on services and their models.
/// </summary>
internal static class Documentation
{

    internal static string? GetSummary(MemberInfo member) => member.GetCustomAttribute<SummaryAttribute>(true)?.Text;

    internal static string? GetSummary(ParameterInfo parameter) => parameter.GetCustomAttribute<SummaryAttribute>(true)?.Text;

    internal static string? GetRemarks(MemberInfo member) => member.GetCustomAttribute<RemarksAttribute>(true)?.Text;

    /// <summary>
    /// Combines summary and remarks of the given member into a single text,
    /// for targets that do not distinguish between them.
    /// </summary>
    internal static string? GetDescription(MemberInfo member) => Combine(GetSummary(member), GetRemarks(member));

    internal static string? Combine(string? summary, string? remarks)
    {
        if (summary == null)
        {
            return remarks;
        }

        return remarks == null ? summary : $"{summary}\n\n{remarks}";
    }

}
