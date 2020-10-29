using System;
using System.IO;
using System.Text;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.IO.Strings
{

    public class StringResource : IResource
    {
        private static readonly Encoding UTF8 = Encoding.UTF8;

        #region Get-/Setters

        public string Content { get; }

        public string? Name { get; }

        public DateTime? Modified { get; }

        public FlexibleContentType? ContentType { get; }

        public ulong? Length => (ulong)Content.Length;

        #endregion

        #region Initialization

        public StringResource(string content, string? name, FlexibleContentType? contentType, DateTime? modified)
        {
            Content = content;

            Name = name;
            ContentType = contentType;
            Modified = modified;
        }

        #endregion

        #region Functionality

        public Stream GetContent() => OptimizedStream.From(UTF8.GetBytes(Content));

        public ulong GetChecksum() => (ulong)Content.GetHashCode();

        #endregion

    }

}
