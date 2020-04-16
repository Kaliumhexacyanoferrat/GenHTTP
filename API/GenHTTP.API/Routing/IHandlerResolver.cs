using GenHTTP.Api.Content;

namespace GenHTTP.Api.Routing
{

    public interface IHandlerResolver
    {

        IHandler? Find(string segment);

    }

}
