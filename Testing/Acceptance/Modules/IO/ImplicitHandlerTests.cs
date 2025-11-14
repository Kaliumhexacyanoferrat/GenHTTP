using System.Net;

using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO;

using StringContent = GenHTTP.Modules.IO.Strings.StringContent;

namespace GenHTTP.Testing.Acceptance.Modules.IO;

[TestClass]
public class ImplicitHandlerTests
{
    
    [TestMethod]
    public async Task TestHandlerAsync()
    {
        var handler = Handler.From((r) => Content.From(Resource.FromString("Hello World")).Build().HandleAsync(r));
        
        await using var runner = await TestHost.RunAsync(handler);

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);
        
        Assert.AreEqual("Hello World", await response.GetContentAsync());
    }
    
    [TestMethod]
    public async Task TestHandlerSync()
    {
        var handler = Handler.From((r) =>
        {
            return r.Respond()
                    .Content(new StringContent("Hello World"))
                    .Type(ContentType.TextPlain)
                    .Build();
        });
        
        await using var runner = await TestHost.RunAsync(handler);

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);
        
        Assert.AreEqual("Hello World", await response.GetContentAsync());
    }
    
}
