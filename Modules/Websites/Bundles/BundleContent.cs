using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Websites.Bundles
{

    public class BundleContent : IResponseContent
    {
        private readonly byte[] _NewLine = new byte[] { (byte)'\n' };

        #region Get-/Setters

        public ulong? Length => null;

        private IEnumerable<IResourceProvider> Items { get; }

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
