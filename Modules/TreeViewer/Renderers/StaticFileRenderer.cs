using System;
using System.IO;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.TreeViewer.Renderers
{

    public class StaticFileRenderer : ITreeRenderer
    {

        #region Get-/Setters

        public string[] IndexNames { get; }

        #endregion

        #region Initialization

        public StaticFileRenderer(string[] indexNames)
        {
            IndexNames = indexNames;
        }

        #endregion

        #region Functionality

        public async ValueTask<IHandlerBuilder?> GetIndexAsync(IResourceContainer container)
        {
            await foreach (var file in container.GetResources())
            {
                if (IsIndex(file))
                {
                    return await RenderAsync(file);
                }
            }

            return null;
        }

        public async ValueTask<TreeViewItem?> GetHandlerAsync(IResourceContainer container, IResource resource)
        {
            if (!IsIndex(resource))
            {
                var fileName = GetFileName(resource);

                if (fileName != null)
                {
                    var handler = await RenderAsync(resource);

                    if (handler != null)
                    {
                        return new(fileName, handler);
                    }
                }
            }

            return null;
        }

        private bool IsIndex(IResource resource)
        {
            var fileName = GetFileName(resource);

            foreach (var indexFile in IndexNames)
            {
                if (string.Equals(indexFile, fileName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string? GetFileName(IResource resource)
        {
            if (resource.Name != null)
            {
                return Path.GetFileNameWithoutExtension(resource.Name);

            }

            return null;
        }

        private ValueTask<IHandlerBuilder?> RenderAsync(IResource resource)
        {
            // ToDo
            return null;
        }

        #endregion

    }

}
