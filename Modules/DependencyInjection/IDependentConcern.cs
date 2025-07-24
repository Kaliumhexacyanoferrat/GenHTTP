using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.DependencyInjection;

public interface IDependentConcern
{

    ValueTask<IResponse?> HandleAsync(IHandler content, IRequest request);

}
