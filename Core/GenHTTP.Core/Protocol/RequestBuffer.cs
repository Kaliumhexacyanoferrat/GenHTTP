using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;

using GenHTTP.Core.Infrastructure.Configuration;

namespace GenHTTP.Core.Protocol
{

    internal class RequestBuffer
    {

        #region Get-/Setters

        private PipeReader Reader { get; }

        private NetworkConfiguration Configuration { get; }

        internal ReadOnlySequence<byte> Data { get; private set; }

        #endregion

        #region Initialization

        internal RequestBuffer(PipeReader reader, NetworkConfiguration configuration)
        {
            Reader = reader;
            Configuration = configuration;

            Data = new ReadOnlySequence<byte>();
        }

        #endregion

        #region Functionality

        internal async ValueTask<long?> Read(bool force = false)
        {
            if ((Data.Length == 0) || force)
            {
                var data = await Reader.ReadWithTimeoutAsync(Configuration.RequestReadTimeout);

                if (data != null)
                {
                    Data = data.Value.Buffer;
                }
                else
                {
                    return null;
                }
            }

            return Data.Length;
        }

        internal void Advance(SequencePosition position)
        {
            Data = Data.Slice(position);
            Acknowledge();
        }

        internal void Advance(long bytes)
        {
            Data = Data.Slice(bytes);
            Acknowledge();
        }

        internal void Acknowledge()
        {
            Reader.AdvanceTo(Data.Start); // , Data.End
        }

        #endregion

    }

}
