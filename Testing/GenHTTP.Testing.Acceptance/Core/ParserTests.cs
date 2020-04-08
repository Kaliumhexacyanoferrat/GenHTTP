using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Core.General;
using GenHTTP.Testing.Acceptance.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace GenHTTP.Testing.Acceptance.Core
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
                              .Content(new StringContent(request.Target.Path.ToString()))
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
                              .Content(new StringContent(string.Join('|', request.Query.Select(kv => kv.Key + "=" + kv.Value))))
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
            using var runner = TestRunner.Run(new PathReturner().Wrap());

            using var respose = runner.GetResponse("/?söme key=💕");

            Assert.Equal("söme key=💕", respose.GetContent());
        }

    }

}
