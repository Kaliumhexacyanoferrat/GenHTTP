using System.Collections.Generic;

namespace GenHTTP.Api.Content.IO
{

    public interface IResourceNode
    {

        bool IsRoot { get; }

        IResourceNode Parent { get; }

        IDictionary<string, IResourceNode> GetNodes();

        IDictionary<string, IResource> GetResources();

    }

}
