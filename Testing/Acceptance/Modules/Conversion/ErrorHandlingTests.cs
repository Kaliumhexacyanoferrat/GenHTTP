﻿using System.Net;
using System.Net.Http.Headers;
using GenHTTP.Modules.Functional;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Conversion;

[TestClass]
public class ErrorHandlingTests
{

    #region Tests

    [TestMethod]
    public async Task UndeserializableBodyReturnsWithBadRequest()
    {
        var inline = Inline.Create()
                           .Post("/t", (MyEntity entity) => entity.Data);

        using var runner = TestHost.Run(inline);

        using var request = runner.GetRequest("/t");

        request.Method = HttpMethod.Post;

        request.Content = new StringContent("I cannot be deserialized", null, "application/json");
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        using var response = await runner.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Supporting data structures

    private record MyEntity(string Data);

    #endregion

}
