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

            public IEnumerable<ContentElement> GetContent(IRequest request)
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

            public IEnumerable<ContentElement> GetContent(IRequest request)
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
        public void TestEndodedUri()
        {
            using var runner = TestRunner.Run(new PathReturner().Wrap());

            using var respose = runner.GetResponse("/söme/ürl/with specialities/");

            Assert.AreEqual("/söme/ürl/with specialities/", respose.GetContent());
        }

        [TestMethod]
        public void TestEncodedQuery()
        {
            using var runner = TestRunner.Run(new QueryReturner().Wrap());

            using var respose = runner.GetResponse("/?söme key=💕");

            Assert.AreEqual("söme key=💕", respose.GetContent());
        }

        [TestMethod]
        public void TestMultipleSlashes()
        {
            using var runner = TestRunner.Run(new PathReturner().Wrap());

            using var respose = runner.GetResponse("//one//two//three//");

            Assert.AreEqual("//one//two//three//", respose.GetContent());
        }

        [TestMethod]
        public void TestEmptyQuery()
        {
            using var runner = TestRunner.Run(new QueryReturner().Wrap());

            using var respose = runner.GetResponse("/?");

            Assert.AreEqual(string.Empty, respose.GetContent());
        }

        [TestMethod]
        public void TestUnkeyedQuery()
        {
            using var runner = TestRunner.Run(new QueryReturner().Wrap());

            using var respose = runner.GetResponse("/?query");

            Assert.AreEqual("query=", respose.GetContent());
        }

        [TestMethod]
        public void TestQueryWithSlashes()
        {
            using var runner = TestRunner.Run(new QueryReturner().Wrap());

            using var respose = runner.GetResponse("/?key=/one/two");

            Assert.AreEqual("key=/one/two", respose.GetContent());
        }

        [TestMethod]
        public void TestQueryWithSpaces()
        {
            using var runner = TestRunner.Run(new QueryReturner().Wrap());

            using var respose = runner.GetResponse("/?path=/Some+Folder/With%20Subfolders/");

            Assert.AreEqual("path=/Some+Folder/With Subfolders/", respose.GetContent());
        }

    }

}
