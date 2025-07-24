using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.DependencyInjection;

public interface IDependentHandler
{

    ValueTask<IResponse?> HandleAsync(IRequest request);

}
