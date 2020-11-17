using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.Websites.Bundles
{

    public class BundleContent : IResponseContent
    {
        private readonly byte[] _NewLine = new byte[] { (byte)'\n' };

        #region Get-/Setters

        public ulong? Length => null;

        private IEnumerable<IResource> Items { get; }

        #endregion

        #region Initialization

        public BundleContent(IEnumerable<IResource> items)
        {
            Items = items;
        }

        #endregion

        #region Functionality

        public async ValueTask WriteAsync(Stream target, uint bufferSize)
        {
            foreach (var item in Items)
            {
                using var source = await item.GetContentAsync();

                await source.CopyPooledAsync(target, bufferSize);

                await target.WriteAsync(_NewLine, 0, 1);
            }
        }

        public async ValueTask<ulong?> CalculateChecksumAsync()
        {
            unchecked
            {
                ulong hash = 17;

                foreach (var item in Items)
                {
                    using var source = await item.GetContentAsync();

                    var checksum = await source.CalculateChecksumAsync();

                    if (checksum is not null)
                    {
                        hash = hash * 23 + (ulong)checksum;
                    }
                    else
                    {
                        return null;
                    }
                }

                return hash;
            }
        }

        #endregion

    }

}
