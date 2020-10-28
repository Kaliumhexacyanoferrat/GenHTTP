using System.Reflection;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.IO.Embedded
{

    public class EmbeddedResourceTreeBuilder : IBuilder<IResourceTree>
    {
        private string? _Root;

        private Assembly? _Source;

        #region Functionality

        public EmbeddedResourceTreeBuilder Source(Assembly source)
        {
            _Source = source;
            return this;
        }

        public EmbeddedResourceTreeBuilder Root(string root)
        {
            _Root = root;
            return this;
        }

        public IResourceTree Build()
        {
            var source = _Source ?? throw new BuilderMissingPropertyException("source");

            var root = _Root ?? throw new BuilderMissingPropertyException("root");

            return new EmbeddedResourceTree(source, root);
        }

        #endregion

    }

}
