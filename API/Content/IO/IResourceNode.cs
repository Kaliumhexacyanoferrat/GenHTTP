namespace GenHTTP.Api.Content.IO
{

    public interface IResourceNode : IResourceContainer
    {

        string Name { get; }

        IResourceContainer Parent { get; }

    }

}
