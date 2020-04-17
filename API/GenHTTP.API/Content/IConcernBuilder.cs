using System;

namespace GenHTTP.Api.Content
{

    public interface IConcernBuilder
    {

        IConcern Build(IHandler parent, Func<IHandler, IHandler> contentFactory);

    }

}
