using System.Net;
using System.Net.Http.Headers;
using System.Runtime.ExceptionServices;
using System.Text;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.Conversion.Serializers.Yaml;
using GenHTTP.Modules.Functional;

using YamlDotNet.Core;

namespace GenHTTP.Testing.Acceptance.Modules.Conversion;

[TestClass]
public sealed class YamlDepthTests
{
    private const int MaximumDepth = 32;

    private const int StackSize = 1536 * 1024;

    #region Supporting data structures

    public sealed class Node
    {

        public Node? Child { get; set; }

    }

    #endregion

    #region Tests

    [TestMethod]
    public void TestDocumentAtMaximumDepthIsAccepted()
    {
        var node = Deserialize(Nest(MaximumDepth - 1));

        var depth = 0;

        while (node != null)
        {
            depth++;
            node = node.Child;
        }

        Assert.AreEqual(MaximumDepth, depth);
    }

    [TestMethod]
    public void TestDocumentAboveMaximumDepthIsRejected()
    {
        Assert.ThrowsExactly<YamlException>(() => Deserialize(Nest(MaximumDepth)));
    }

    [TestMethod]
    public void TestDeeplyNestedDocumentIsRejected()
    {
        Assert.ThrowsExactly<YamlException>(() => Deserialize(Nest(100)));
    }

    [TestMethod]
    public void TestVeryDeeplyNestedDocumentIsRejected()
    {
        Assert.ThrowsExactly<YamlException>(() => Deserialize(Nest(100_000)));
    }

    [TestMethod]
    public void TestDeeplyNestedSequenceIsRejected()
    {
        var yaml = new string('[', 100_000) + new string(']', 100_000);

        Assert.ThrowsExactly<YamlException>(() => Deserialize(yaml));
    }

    [TestMethod]
    public void TestUnknownPropertyAtMaximumDepthIsRejected()
    {
        Assert.ThrowsExactly<YamlException>(() => Deserialize(Nest(MaximumDepth - 1, "unknown: 1")));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestDeeplyNestedBodyReturnsBadRequest(ServerEngine engine)
    {
        var inline = Inline.Create()
                           .Post("/t", (Node node) => node.Child != null);

        await using var runner = await TestHost.RunAsync(inline, engine: engine);

        using var request = runner.GetRequest("/t");

        request.Method = HttpMethod.Post;

        request.Content = new StringContent(Nest(100_000));
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/yaml");

        using var response = await runner.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Helpers

    private static string Nest(int depth, string leaf = "child: null")
    {
        return new StringBuilder().Insert(0, "child: {", depth)
                                  .Append(leaf)
                                  .Append('}', depth)
                                  .ToString();
    }

    private static Node? Deserialize(string yaml)
    {
        Node? result = null;
        Exception? error = null;

        var thread = new Thread(() =>
        {
            try
            {
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(yaml));

                result = (Node?)new YamlFormat().DeserializeAsync(stream, typeof(Node)).AsTask().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                error = e;
            }
        }, StackSize);

        thread.Start();
        thread.Join();

        if (error != null)
        {
            ExceptionDispatchInfo.Throw(error);
        }

        return result;
    }

    #endregion

}
