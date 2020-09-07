using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Conversion.Providers.Xml
{

    public class XmlContent : IResponseContent
    {

        #region Get-/Setters

        public ulong? Length => null;

        private object Data { get; }

        #endregion

        #region Initialization

        public XmlContent(object data)
        {
            Data = data;
        }

        #endregion

        #region Functionality

        public Task Write(Stream target, uint bufferSize)
        {
            var serializer = new XmlSerializer(Data.GetType());
            serializer.Serialize(target, Data);

            return Task.CompletedTask;
        }

        #endregion

    }

}
