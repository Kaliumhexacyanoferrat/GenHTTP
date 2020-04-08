using System.Collections.Generic;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Content
{

    public interface IHandler
    {

        IHandler Parent { get; }

        IEnumerable<ContentElement> GetContent(IRequest request);

        IResponse? Handle(IRequest request);

        // bool Route();

    }

}
