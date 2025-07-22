using System.Net;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance;

/// <summary>
/// Compatibility assertions for XUnit.
/// </summary>
public static class AssertX
{

    public static void Contains(string searchFor, string? content)
    {
        if (content == null || !content.Contains(searchFor))
        {
            throw new AssertFailedException($"String '{searchFor}' is not found in result:\r\n\r\n{content}");
        }
    }

    public static void DoesNotContain(string searchFor, string? content)
    {
        if (content != null && content.Contains(searchFor))
        {
            throw new AssertFailedException($"String '{searchFor}' is found in result:\r\n\r\n{content}");
        }
    }

    public static void StartsWith(string searchFor, string? content) => Assert.IsTrue(content?.StartsWith(searchFor) ?? false);

    public static void EndsWith(string searchFor, string? content) => Assert.IsTrue(content?.EndsWith(searchFor) ?? false);

    public static void Single<T>(IEnumerable<T> collection) => Assert.IsTrue(collection.Count() == 1);

    public static void Empty<T>(IEnumerable<T>? collection) => Assert.IsFalse(collection?.Any() ?? false);

    public static void Contains<T>(T value, IEnumerable<T> collection) => Assert.IsTrue(collection.Contains(value));

    public static void DoesNotContain<T>(T value, IEnumerable<T> collection) => Assert.IsFalse(collection.Contains(value));

    public static void IsNullOrEmpty(string? value) => Assert.IsTrue(string.IsNullOrEmpty(value));

    /// <summary>
    /// Raises an assertion expection if the response does not have the expected status code
    /// and additionally prints information about the response to be able to further debug
    /// issues in workflow runs.
    /// </summary>
    /// <param name="response">The response to be evaluated</param>
    /// <param name="expectedStatus">The expected status code to check for</param>
    public static async Task AssertStatusAsync(this HttpResponseMessage response, HttpStatusCode expectedStatus)
    {
        if (response.StatusCode != expectedStatus)
        {
            var builder = new StringBuilder();

            builder.AppendLine($"Response returned with status '{response.StatusCode}', expected '{expectedStatus}'.");
            builder.AppendLine();

            builder.AppendLine("Headers");
            builder.AppendLine();

            foreach (var header in response.Headers)
            {
                builder.AppendLine($"  {header.Key} = {string.Join(',', header.Value.ToList())}");
            }

            builder.AppendLine();

            var content = await response.Content.ReadAsStringAsync();

            if (!string.IsNullOrEmpty(content))
            {
                builder.AppendLine("Body");
                builder.AppendLine();

                builder.AppendLine(content);
            }

            throw new AssertFailedException(builder.ToString());
        }
    }
}
