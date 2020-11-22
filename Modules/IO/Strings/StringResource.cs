using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Strings
{

    public sealed class StringResource : IResource
    {
        private static readonly Encoding UTF8 = Encoding.UTF8;

        #region Get-/Setters

        public byte[] Content { get; }

        public string? Name { get; }

        public DateTime? Modified { get; }

        public FlexibleContentType? ContentType { get; }

        public ulong? Length => (ulong)Content.Length;

        #endregion

        #region Initialization

        public StringResource(string content, string? name, FlexibleContentType? contentType, DateTime? modified)
        {
            Content = UTF8.GetBytes(content);

            Name = name;
            ContentType = contentType ?? new FlexibleContentType(Api.Protocol.ContentType.TextPlain);
            Modified = modified;
        }

        #endregion

        #region Functionality

        public ValueTask<Stream> GetContentAsync() => new ValueTask<Stream>(new MemoryStream(Content));

        public ValueTask<ulong> CalculateChecksumAsync()
        {
            return new ValueTask<ulong>((ulong)Content.GetHashCode());
        }

        #endregion

    }

}
