using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.DirectoryBrowsing.Provider
{

    public sealed class ListingModel : AbstractModel
    {

        #region Get-/Setters

        public IResourceContainer Container { get; }

        public bool HasParent { get; }

        #endregion

        #region Intialization

        public ListingModel(IRequest request, IHandler handler, IResourceContainer container, bool hasParent) : base(request, handler)
        {
            Container = container;
            HasParent = hasParent;
        }

        #endregion

        #region Functionality

        public override async ValueTask<ulong> CalculateChecksumAsync()
        {
            unchecked
            {
                ulong hash = 17;

                hash = hash * 23 + (uint)(HasParent ? 1 : 0);

                await foreach (var node in Container.GetNodes())
                {
                    hash = hash * 23 + (uint)node.Name.GetHashCode();
                    hash = hash * 23 + (uint)(node.Modified?.GetHashCode() ?? 0);
                }

                await foreach (var resource in Container.GetResources())
                {
                    hash = hash * 23 + await resource.CalculateChecksumAsync();
                }

                return hash;
            }
        }

        #endregion

    }

}
