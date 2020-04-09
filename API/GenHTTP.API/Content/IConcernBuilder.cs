using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Api.Content
{

    public interface IConcernBuilder
    {

        IConcern Build(IHandler parent, Func<IHandler, IHandler> contentFactory);

    }

}
