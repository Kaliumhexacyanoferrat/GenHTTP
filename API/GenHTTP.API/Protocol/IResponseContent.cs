using System.IO;
using System.Threading.Tasks;

namespace GenHTTP.Api.Protocol
{

    public interface IResponseContent
    {
        
        ulong? Length { get; }

        Task Write(Stream target, uint bufferSize);

    }

}
