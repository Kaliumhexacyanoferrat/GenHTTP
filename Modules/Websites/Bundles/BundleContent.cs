using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.Websites.Bundles
{

    public class BundleContent : IResponseContent
    {
        private readonly byte[] _NewLine = new byte[] { (byte)'\n' };

        #region Get-/Setters

        public ulong? Length => null;

        private IEnumerable<IResourceProvider> Items { get; }

        public ulong? Checksum
        {
            get
            {
                unchecked
                {
                    ulong hash = 17;

                    foreach (var item in Items)
                    {
                        using var source = item.GetResource();

                        var checksum = source.CalculateChecksum();

                        if (checksum != null)
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
        }

        #endregion

        #region Initialization

        public BundleContent(IEnumerable<IResourceProvider> items)
        {
            Items = items;
        }

        #endregion

        #region Functionality

        public async Task Write(Stream target, uint bufferSize)
        {
            foreach (var item in Items)
            {
                using var source = item.GetResource();
                await source.CopyToAsync(target, (int)bufferSize);

                await target.WriteAsync(_NewLine, 0, 1);
            }
        }

        #endregion

    }

}
