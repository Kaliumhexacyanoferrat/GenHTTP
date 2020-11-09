using System.IO;
using System.Threading.Tasks;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using PooledAwait;

namespace GenHTTP.Modules.IO.Streaming
{

    public class ResourceContent : IResponseContent
    {

        #region Get-/Setters

        public ulong? Length => Resource.Length;

        private IResource Resource { get; }

        #endregion

        #region Initialization

        public ResourceContent(IResource resource)
        {
            Resource = resource;
        }

        #endregion

        #region Functionality

        public async ValueTask<ulong?> CalculateChecksumAsync() => await Resource.CalculateChecksumAsync();

        public ValueTask WriteAsync(Stream target, uint bufferSize)
        {
            return DoWrite(this, target, bufferSize);

            static async PooledValueTask DoWrite(ResourceContent self, Stream target, uint bufferSize)
            {
                using var source = await self.Resource.GetContentAsync();

                await source.CopyPooledAsync(target, bufferSize);
            }
        }

        #endregion

    }

}
