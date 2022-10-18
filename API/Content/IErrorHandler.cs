using System;
using System.Threading.Tasks;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Content
{

    public interface IErrorHandler<in T> where T : Exception
    {

        ValueTask<IResponse?> Map(IRequest request, IHandler handler, T error);

        IResponse? GetNotFound(IRequest request, IHandler handler);

    }

}
