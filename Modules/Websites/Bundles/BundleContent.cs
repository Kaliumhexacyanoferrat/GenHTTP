using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Websites.Bundles
{

    public sealed class BundleContent : IResponseContent
    {
        private static readonly byte[] _NewLine = new byte[] { (byte)'\n' };

        #region Get-/Setters

        public ulong? Length
        {
            get
            {
                if (Items.Count > 0)
                {
                    ulong length = 0;

                    foreach (var item in Items)
                    {
                        var itemLength = item.Length;

                        if (itemLength is null)
                        {
                            return null;
                        }

                        length += itemLength.Value;
                    }

                    return length + (ulong)(Items.Count - 1);
                }

                return 0;
            }
        }

        private List<IResource> Items { get; }

        #endregion

        #region Initialization

        public BundleContent(IEnumerable<IResource> items)
        {
            Items = items.ToList();
        }

        #endregion

        #region Functionality

        public async ValueTask WriteAsync(Stream target, uint bufferSize)
        {
            foreach (var item in Items)
            {
                await item.WriteAsync(target, bufferSize);

                await target.WriteAsync(_NewLine.AsMemory());
            }
        }

        public async ValueTask<ulong?> CalculateChecksumAsync()
        {
            unchecked
            {
                ulong hash = 17;

                foreach (var item in Items)
                {
                    hash = hash * 23 + await item.CalculateChecksumAsync();
                }

                return hash;
            }
        }

        #endregion

    }

}
