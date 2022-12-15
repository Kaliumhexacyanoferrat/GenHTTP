using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Authentication;
using GenHTTP.Modules.IO;

namespace GenHTTP.Testing.Acceptance.Modules.Authentication
{

    public class UserReturningHandlerBuilder : IHandlerBuilder
    {

        public IHandler Build(IHandler parent) => new UserReturningHandler(parent);

    }

    public class UserReturningHandler : IHandler
    {

        public ValueTask PrepareAsync() => ValueTask.CompletedTask;

        public IHandler Parent { get; }

        public UserReturningHandler(IHandler parent)
        {
            Parent = parent;
        }

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request) => AsyncEnumerable.Empty<ContentElement>();

        public ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var content = request.GetUser<IUser>()?.DisplayName ?? throw new ProviderException(ResponseStatus.BadRequest, "No user!");

            return request.Respond()
                          .Content(content)
                          .BuildTask();
        }

    }

}
