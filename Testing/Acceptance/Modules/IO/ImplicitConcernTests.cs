using System.Net;
using GenHTTP.Modules.IO;

namespace GenHTTP.Testing.Acceptance.Modules.IO;

[TestClass]
public class ImplicitConcernTests
{

    [TestMethod]
    public async Task TestConcern()
    {
        var app = Content.From(Resource.FromString("Hello World"))
                         .Add(Concern.From(async (r, c) =>
                         {
                             var response = await c.HandleAsync(r);

                             response!.Headers.Add("X-Custom", "Value");
                             
                             return response;
                         }));
        
        await using var runner = await TestHost.RunAsync(app);

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);
        
        Assert.AreEqual("Hello World", await response.GetContentAsync());
        
        Assert.AreEqual("Value", response.GetHeader("X-Custom"));
    }
    
}
