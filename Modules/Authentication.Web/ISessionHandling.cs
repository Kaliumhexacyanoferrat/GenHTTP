using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Authentication.Web
{

    public interface ISessionHandling
    {

        string? ReadToken(IRequest request);

        void WriteToken(IResponse response, string sessionToken);

        void ClearToken(IResponse response);

    }

}
