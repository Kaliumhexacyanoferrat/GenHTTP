using System.Collections.Generic;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Content.Websites
{

    public interface IMenuProvider
    {

        List<ContentElement> GetMenu(IRequest request);

    }

}
