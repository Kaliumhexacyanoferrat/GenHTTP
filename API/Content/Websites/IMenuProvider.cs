using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Content.Websites
{

    public interface IMenuProvider
    {

        ValueTask<List<ContentElement>> GetMenuAsync(IRequest request, IHandler handler);

    }

}
