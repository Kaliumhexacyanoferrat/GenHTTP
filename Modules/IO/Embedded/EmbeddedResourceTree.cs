using System.Reflection;
using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.IO.Embedded;

internal class EmbeddedResourceTree : EmbeddedResourceContainer, IResourceTree
{

    internal EmbeddedResourceTree(Assembly source, string root) : base(source, root) { }
}
