using GenHTTP.Api.Content;

namespace GenHTTP.Examples.Examples.Infrastructure
{

    public class ExceptionProviderBuilder : IHandlerBuilder
    {

        public IHandler Build(IHandler parent)
        {
            return new ExceptionProvider(parent);
        }

    }

}
