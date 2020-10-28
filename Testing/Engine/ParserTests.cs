using System;
using System.Collections.Generic;
using System.Linq;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

using Xunit;

namespace GenHTTP.Testing.Acceptance.Engine
{

    public class ParserTests
    {

        #region Supporting data structures

        private class PathReturner : IHandler
        {

            public IHandler Parent => throw new NotImplementedException();

            public IEnumerable<ContentElement> GetContent(IRequest request)
            {
                throw new NotImplementedException();
            }

            public IResponse? Handle(IRequest request)
            {
                return request.Respond()
                              .Content(request.Target.Path.ToString())
                              .Build();
            }

        }

        private class QueryReturner : IHandler
        {

            public IHandler Parent => throw new NotImplementedException();

            public IEnumerable<ContentElement> GetContent(IRequest request)
            {
                throw new NotImplementedException();
            }

            public IResponse? Handle(IRequest request)
            {
                return request.Respond()
                              .Content(string.Join('|', request.Query.Select(kv => kv.Key + "=" + kv.Value)))
                              .Build();
            }

        }

        #endregion

        [Fact]
        public void TestEndodedUri()
        {
            using var runner = TestRunner.Run(new PathReturner().Wrap());

            using var respose = runner.GetResponse("/söme/ürl/with specialities/");

            Assert.Equal("/söme/ürl/with specialities/", respose.GetContent());
        }

        [Fact]
        public void TestEncodedQuery()
        {
            using var runner = TestRunner.Run(new QueryReturner().Wrap());

            using var respose = runner.GetResponse("/?söme key=💕");

            Assert.Equal("söme key=💕", respose.GetContent());
        }

        [Fact]
        public void TestMultipleSlashes()
        {
            using var runner = TestRunner.Run(new PathReturner().Wrap());

            using var respose = runner.GetResponse("//one//two//three//");

            Assert.Equal("//one//two//three//", respose.GetContent());
        }

        [Fact]
        public void TestEmptyQuery()
        {
            using var runner = TestRunner.Run(new QueryReturner().Wrap());

            using var respose = runner.GetResponse("/?");

            Assert.Equal(string.Empty, respose.GetContent());
        }

        [Fact]
        public void TestUnkeyedQuery()
        {
            using var runner = TestRunner.Run(new QueryReturner().Wrap());

            using var respose = runner.GetResponse("/?query");

            Assert.Equal("query=", respose.GetContent());
        }

        [Fact]
        public void TestQueryWithSlashes()
        {
            using var runner = TestRunner.Run(new QueryReturner().Wrap());

            using var respose = runner.GetResponse("/?key=/one/two");

            Assert.Equal("key=/one/two", respose.GetContent());
        }

        [Fact]
        public void TestQueryWithSpaces()
        {
            using var runner = TestRunner.Run(new QueryReturner().Wrap());

            using var respose = runner.GetResponse("/?path=/Some+Folder/With%20Subfolders/");

            Assert.Equal("path=/Some+Folder/With Subfolders/", respose.GetContent());
        }

    }

}
