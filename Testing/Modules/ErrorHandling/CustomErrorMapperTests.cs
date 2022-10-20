using System;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.ErrorHandling;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.ErrorHandling
{

    [TestClass]
    public sealed class CustomErrorMapperTests
    {

        #region Supporting data structures

        public class ErrorLengthMapper : IErrorMapper<Exception>
        {
            public ValueTask<IResponse?> GetNotFound(IRequest request, IHandler handler)
            {
                return new
                (
                    request.Respond()
                           .Status(ResponseStatus.NotFound)
                           .Content(Resource.FromString("404").Build())
                           .Build()
                );
            }

            public ValueTask<IResponse?> Map(IRequest request, IHandler handler, Exception error)
            {
                return new
                (
                    request.Respond()
                           .Status(ResponseStatus.NotFound)
                           .Content(Resource.FromString($"{error.Message.Length}").Build())
                           .Build()
                );
            }
        }

        #endregion

        #region Tests

        [TestMethod]
        public async Task Test404Mapped()
        {
            var test = Layout.Create()
                             .Add(ErrorHandler.From(new ErrorLengthMapper()));

            using var runner = TestRunner.Run(test);

            using var response = await runner.GetResponse("/");
            Assert.AreEqual("404", await response.GetContent());
        }

        [TestMethod]
        public async Task TestExceptionMapped()
        {
            Action thrower = () => throw new Exception("Nope!");

            var test = Layout.Create()
                             .Add(Inline.Create().Get(thrower))
                             .Add(ErrorHandler.From(new ErrorLengthMapper()));

            using var runner = TestRunner.Run(test);

            using var response = await runner.GetResponse("/");
            Assert.AreEqual("5", await response.GetContent());
        }

        #endregion

    }

}
