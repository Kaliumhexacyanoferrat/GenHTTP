using GenHTTP.Api.Content;

namespace GenHTTP.Modules.ClientCaching.Validation;

public sealed class CacheValidationBuilder : IConcernBuilder
{

    public IConcern Build(IHandler parent, Func<IHandler, IHandler> contentFactory)
    {
            return new CacheValidationHandler(parent, contentFactory);
        }

}
