using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GenHTTP.Api.Content.IO
{

    public interface IResourceContainer
    {

        bool TryGetNode(string name, [MaybeNullWhen(returnValue: false)] out IResourceNode node);

        IEnumerable<IResourceNode> GetNodes();

        bool TryGetResource(string name, [MaybeNullWhen(returnValue: false)] out IResource node);

        IEnumerable<IResource> GetResources();

    }

}
