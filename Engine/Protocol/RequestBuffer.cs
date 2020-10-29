using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading;

using GenHTTP.Engine.Infrastructure.Configuration;

using PooledAwait;

namespace GenHTTP.Engine.Protocol
{

    internal class RequestBuffer
    {

        #region Get-/Setters

        private PipeReader Reader { get; }

        private NetworkConfiguration Configuration { get; }

        internal ReadOnlySequence<byte> Data { get; private set; }

        internal bool ReadRequired => Data.IsEmpty;

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

        internal async PooledValueTask<long?> Read(bool force = false)
        {
            if ((Data.Length == 0) || force)
            {
                using var cancellation = new CancellationTokenSource(Configuration.RequestReadTimeout);

                try
                {
                    Data = (await Reader.ReadAsync(cancellation.Token).ConfigureAwait(false)).Buffer;
                }
                catch (OperationCanceledException)
                {
                    return null;
                }
            }

            return Data.Length;
        }

        internal void Advance(SequencePosition position)
        {
            Data = Data.Slice(position);
            Reader.AdvanceTo(Data.Start);
        }

        internal void Advance(long bytes)
        {
            Data = Data.Slice(bytes);
            Reader.AdvanceTo(Data.Start);
        }

        #endregion

    }

}
