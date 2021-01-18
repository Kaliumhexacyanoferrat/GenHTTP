using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading;

using GenHTTP.Engine.Infrastructure.Configuration;

using PooledAwait;

namespace GenHTTP.Engine.Protocol
{

    internal sealed class RequestBuffer : IDisposable
    {

        #region Get-/Setters

        private PipeReader Reader { get; }

        private NetworkConfiguration Configuration { get; }

        private CancellationTokenSource? Cancellation { get; set; }

        internal ReadOnlySequence<byte> Data { get; private set; }

        internal bool ReadRequired => Data.IsEmpty;

        #endregion

        #region Initialization

        internal RequestBuffer(PipeReader reader, NetworkConfiguration configuration)
        {
            Reader = reader;
            Configuration = configuration;

            Data = new();
        }

        #endregion

        #region Functionality

        internal async PooledValueTask<long?> Read(bool force = false)
        {
            if ((Data.Length == 0) || force)
            {
                if (Cancellation is null)
                {
                    Cancellation = new();
                }

                try
                {
                    Cancellation.CancelAfter(Configuration.RequestReadTimeout);

                    Data = (await Reader.ReadAsync(Cancellation.Token).ConfigureAwait(false)).Buffer;

                    Cancellation.CancelAfter(int.MaxValue);
                }
                catch (OperationCanceledException)
                {
                    Cancellation.Dispose();
                    Cancellation = null;

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

        #region Disposing 

        private bool disposedValue;

        public void Dispose()
        {
            if (!disposedValue)
            {
                if (Cancellation is not null)
                {
                    Cancellation.Dispose();
                    Cancellation = null;
                }

                disposedValue = true;
            }

            GC.SuppressFinalize(this);
        }

        #endregion

    }

}
