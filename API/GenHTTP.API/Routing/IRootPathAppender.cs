using GenHTTP.Api.Content;

namespace GenHTTP.Api.Routing
{
    
    public interface IRootPathAppender
    {

        void Append(PathBuilder path, IHandler? child = null);

    }

}
