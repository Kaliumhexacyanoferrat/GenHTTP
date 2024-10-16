﻿using System.Net;
using System.Net.Http.Headers;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Authentication;
using GenHTTP.Modules.Authentication.Bearer;
using GenHTTP.Modules.Functional;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Authentication;

[TestClass]
public sealed class BearerAuthenticationTests
{
    private const string ValidToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";

    [TestMethod]
    public async Task TestValidToken()
    {
        var auth = BearerAuthentication.Create()
                                       .AllowExpired();

        using var response = await Execute(auth, ValidToken);

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("Secured", await response.GetContentAsync());
    }

    [TestMethod]
    public async Task TestCustomValidator()
    {
        var auth = BearerAuthentication.Create()
                                       .Validation(_ => throw new ProviderException(ResponseStatus.Forbidden, "Nah"))
                                       .AllowExpired();

        using var response = await Execute(auth, ValidToken);

        await response.AssertStatusAsync(HttpStatusCode.Forbidden);
    }

    [TestMethod]
    public async Task TestNoUser()
    {
        var auth = BearerAuthentication.Create()
                                       .UserMapping((_, _) => new ValueTask<IUser?>())
                                       .AllowExpired();

        using var response = await Execute(auth, ValidToken);

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task TestUser()
    {
        var auth = BearerAuthentication.Create()
                                       .UserMapping((_, _) => new ValueTask<IUser?>(new MyUser
                                       {
                                           DisplayName = "User Name"
                                       }))
                                       .AllowExpired();

        using var response = await Execute(auth, ValidToken);

        await response.AssertStatusAsync(HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task TestNoToken()
    {
        var auth = BearerAuthentication.Create()
                                       .AllowExpired();

        using var response = await Execute(auth);

        await response.AssertStatusAsync(HttpStatusCode.Unauthorized);
    }

    [TestMethod]
    public async Task TestMalformedToken()
    {
        var auth = BearerAuthentication.Create()
                                       .AllowExpired();

        using var response = await Execute(auth, "Lorem Ipsum");

        await response.AssertStatusAsync(HttpStatusCode.BadRequest);
    }

    private static async Task<HttpResponseMessage> Execute(BearerAuthenticationConcernBuilder builder, string? token = null)
    {
        var handler = Inline.Create()
                            .Get(() => "Secured")
                            .Add(builder);

        using var host = TestHost.Run(handler);

        var request = host.GetRequest();

        if (token != null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await host.GetResponseAsync(request);
    }

    #region Supporting data structures

    internal class MyUser : IUser
    {

        public string DisplayName { get; init; } = "";
    }

    #endregion

}
