﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Controllers;
using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Layouting;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Controllers
{

    [TestClass]
    public sealed class DataTests
    {

        #region Controller

        public class TestController
        {

            [ControllerAction(RequestMethod.POST)]
            public DateOnly Date(DateOnly date) => date;

        }

        #endregion

        #region Tests

        [TestMethod]
        public async Task TestDateOnly()
        {
            using var host = GetHost();

            var request = host.GetRequest("/t/date/", HttpMethod.Post);

            var data = new Dictionary<string, string>()
            {
                { "date", "2024-03-11" }
            };

            request.Content = new FormUrlEncodedContent(data);

            using var response = await host.GetResponseAsync(request);

            await response.AssertStatusAsync(HttpStatusCode.OK);

            Assert.AreEqual("2024-03-11", await response.GetContentAsync());
        }

        #endregion

        #region Helpers

        private static TestHost GetHost()
        {
            var app = Layout.Create()
                            .AddController<TestController>("t", formats: Serialization.Default());

            return TestHost.Run(app);
        }

        #endregion

    }

}
