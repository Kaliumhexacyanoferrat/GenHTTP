using GenHTTP.Api.Content.Authentication;

namespace GenHTTP.Modules.Authentication.Session
{

    public interface ISession
    {

        IUser? User { get; }

    }

}
