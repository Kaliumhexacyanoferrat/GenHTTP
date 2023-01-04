using GenHTTP.Api.Protocol;
using ProtoBuf;
using System.IO;
using System.Threading.Tasks;

namespace GenHTTP.Modules.Protobuf.Providers
{
    public sealed class ProtobufContent : IResponseContent
    {
        #region Get-/Setters

        public ulong? Length => null;

        private object Data { get; }

        #endregion

        #region Initialization

        public ProtobufContent(object data)
        {
            Data = data;
        }

        #endregion

        #region Functionality

        public ValueTask<ulong?> CalculateChecksumAsync()
        {
            return new ValueTask<ulong?>((ulong)Data.GetHashCode());
        }

        public ValueTask WriteAsync(Stream target, uint bufferSize)
        {
            Serializer.Serialize(target, Data);

            return new ValueTask();
        }

        #endregion
    }
}
