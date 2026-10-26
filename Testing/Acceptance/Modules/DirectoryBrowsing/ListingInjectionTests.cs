using System.Net;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.DirectoryBrowsing;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Pages;

namespace GenHTTP.Testing.Acceptance.Modules.DirectoryBrowsing;

[TestClass]
public sealed class ListingInjectionTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestUnknownIntermediateSegmentIsNotFound(ServerEngine engine)
    {
        var tree = VirtualTree.Create()
                              .Add("sub", VirtualTree.Create());

        await using var runner = await TestHost.RunAsync(Listing.From(tree), engine: engine);

        using var response = await runner.GetResponseAsync("/%3Cimg%20src%3Dx%20onerror%3Dalert(1)%3E/x");

        await response.AssertStatusAsync(HttpStatusCode.NotFound);

        AssertX.DoesNotContain("<img", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNamesAreEscaped(ServerEngine engine)
    {
        const string imageName = "<img src=x onerror=alert(1)>.txt";
        const string attributeName = "x\" onmouseover=\"alert(2).txt";

        var tree = VirtualTree.Create()
                              .Add(imageName, Resource.FromString("1").Name(imageName))
                              .Add(attributeName, Resource.FromString("2").Name(attributeName))
                              .Add("<b>dir</b>", VirtualTree.Create());

        await using var runner = await TestHost.RunAsync(Listing.From(tree), engine: engine);

        using var response = await runner.GetResponseAsync("/");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        var content = await response.GetContentAsync();

        AssertX.DoesNotContain("<img", content);
        AssertX.DoesNotContain("<b>", content);
        AssertX.DoesNotContain("\" onmouseover", content);

        AssertX.Contains("&lt;img src=x onerror=alert(1)&gt;.txt", content);
        AssertX.Contains("href=\"./%3Cimg%20src%3Dx%20onerror%3Dalert%281%29%3E.txt\"", content);
        AssertX.Contains("href=\"./x%22%20onmouseover%3D%22alert%282%29.txt\"", content);
        AssertX.Contains("href=\"./%3Cb%3Edir%3C%2Fb%3E/\"", content);
    }

    [TestMethod]
    public async Task TestServerPageEscapesTitle()
    {
        var page = await Renderer.Server.RenderAsync("<script>alert(1)</script>", "<p>Content</p>");

        AssertX.DoesNotContain("<script>", page);
        AssertX.Contains("&lt;script&gt;alert(1)&lt;/script&gt;", page);
        AssertX.Contains("<p>Content</p>", page);
    }

}
