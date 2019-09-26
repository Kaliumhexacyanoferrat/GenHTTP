using System.Collections.Generic;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Modules.Websites
{

    public interface IMenuProvider
    {

        List<ContentElement> GetMenu(IRequest request);

    }

}
