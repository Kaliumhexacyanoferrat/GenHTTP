using System.IO;
using System.Text;
using System.Threading.Tasks;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Providers
{

    public class StringContent : IResponseContent
    {

        #region Get-/Setters

        public ulong? Length => (ulong)Buffer.Length;

        private byte[] Buffer { get; }

        #endregion

        #region Initialization

        public StringContent(string content) : this(content, Encoding.UTF8) { }

        public StringContent(string content, Encoding encoding)
        {
            Buffer = encoding.GetBytes(content);
        }

        #endregion

        #region Functionality

        public async Task Write(Stream target, uint bufferSize)
        {
            await target.WriteAsync(Buffer, 0, Buffer.Length);
        }

        #endregion

    }

}
