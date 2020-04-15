using GenHTTP.Api.Routing;

namespace GenHTTP.Api.Content
{
    
    public interface IRootPathAppender
    {

        void Append(PathBuilder path, IHandler? child = null);

    }

}
