using System.Net;
using System.Text.Json;

using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Functional;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Conversion;

[TestClass]
public class SerializerTests
{

    #region Supporting data structures

    record MyType(string StringValue, int IntValue);

    #endregion

    #region Tests

    [TestMethod]
    public async Task TestCustomJsonOptions()
    {
        var options = new JsonSerializerOptions()
        {
            WriteIndented = false
        };

        var serialization = Serialization.Default(jsonOptions: options);

        var api = Inline.Create()
                        .Get(() => new MyType("123", 456))
                        .Serializers(serialization);

        await using var host = await TestHost.RunAsync(api);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);

        AssertX.DoesNotContain("\n", await response.GetContentAsync());
    }

    #endregion

}
