using System.IO;
using System.Text;
using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.IO.Providers
{

    public class StringDataProvider : IResource
    {

        #region Get-/Setters

        public string Content { get; }

        #endregion

        #region Initialization

        public StringDataProvider(string content)
        {
            Content = content;
        }

        #endregion

        #region Functionality

        public Stream GetContent()
        {
            return OptimizedStream.From(Encoding.UTF8.GetBytes(Content));
        }

        public ulong GetChecksum() => (ulong)Content.GetHashCode();

        #endregion

    }

}
