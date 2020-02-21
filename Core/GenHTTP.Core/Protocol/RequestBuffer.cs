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

        internal bool InitialBuffer { get; private set; }

        #endregion

        #region Initialization

        internal RequestBuffer(PipeReader reader, NetworkConfiguration configuration)
        {
            Reader = reader;
            Configuration = configuration;

            Data = new ReadOnlySequence<byte>();
            InitialBuffer = true;
        }

        #endregion

        #region Functionality

        internal async ValueTask<long?> Read()
        {
            if (Data.Length == 0)
            {
                if (!InitialBuffer)
                {
                    Acknowledge();
                }
                else
                {
                    InitialBuffer = false;
                }

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
        }

        internal void Advance(long bytes)
        {
            Data = Data.Slice(bytes);
        }

        internal void Acknowledge()
        {
            Reader.AdvanceTo(Data.Start, Data.End);
        }

        #endregion

    }

}
