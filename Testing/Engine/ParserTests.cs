using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine
{

    [TestClass]
    public sealed class ParserTests
    {

        #region Supporting data structures

        private class PathReturner : IHandler
        {

            public ValueTask PrepareAsync() => ValueTask.CompletedTask;

            public IHandler Parent => throw new NotImplementedException();

            public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request)
            {
                throw new NotImplementedException();
            }

            public ValueTask<IResponse?> HandleAsync(IRequest request)
            {
                return request.Respond()
                              .Content(request.Target.Path.ToString())
                              .BuildTask();
            }

        }

        private class QueryReturner : IHandler
        {
            
            public ValueTask PrepareAsync() => ValueTask.CompletedTask;

            public IHandler Parent => throw new NotImplementedException();

            public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request)
            {
                throw new NotImplementedException();
            }

            public ValueTask<IResponse?> HandleAsync(IRequest request)
            {
                return request.Respond()
                              .Content(string.Join('|', request.Query.Select(kv => kv.Key + "=" + kv.Value)))
                              .BuildTask();
            }

        }

        #endregion

        [TestMethod]
        public async Task TestEndodedUri()
        {
            using var runner = TestRunner.Run(new PathReturner().Wrap());

            using var respose = await runner.GetResponse("/söme/ürl/with specialities/");

            Assert.AreEqual("/söme/ürl/with specialities/", await respose.GetContent());
        }

        [TestMethod]
        public async Task TestEncodedQuery()
        {
            using var runner = TestRunner.Run(new QueryReturner().Wrap());

            using var respose = await runner.GetResponse("/?söme key=💕");

            Assert.AreEqual("söme key=💕", await respose.GetContent());
        }

        [TestMethod]
        public async Task TestMultipleSlashes()
        {
            using var runner = TestRunner.Run(new PathReturner().Wrap());

            using var respose = await runner.GetResponse("//one//two//three//");

            Assert.AreEqual("//one//two//three//", await respose.GetContent());
        }

        [TestMethod]
        public async Task TestEmptyQuery()
        {
            using var runner = TestRunner.Run(new QueryReturner().Wrap());

            using var respose = await runner.GetResponse("/?");

            Assert.AreEqual(string.Empty, await respose.GetContent());
        }

        [TestMethod]
        public async Task TestUnkeyedQuery()
        {
            using var runner = TestRunner.Run(new QueryReturner().Wrap());

            using var respose = await runner.GetResponse("/?query");

            Assert.AreEqual("query=", await respose.GetContent());
        }

        [TestMethod]
        public async Task TestQueryWithSlashes()
        {
            using var runner = TestRunner.Run(new QueryReturner().Wrap());

            using var respose = await runner.GetResponse("/?key=/one/two");

            Assert.AreEqual("key=/one/two", await respose.GetContent());
        }

        [TestMethod]
        public async Task TestQueryWithSpaces()
        {
            using var runner = TestRunner.Run(new QueryReturner().Wrap());

            using var respose = await runner.GetResponse("/?path=/Some+Folder/With%20Subfolders/");

            Assert.AreEqual("path=/Some+Folder/With Subfolders/", await respose.GetContent());
        }

    }

}
