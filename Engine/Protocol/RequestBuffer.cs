using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.Http.Headers;
using System.Threading;

using GenHTTP.Engine.Infrastructure.Configuration;

using PooledAwait;

namespace GenHTTP.Engine.Protocol
{

    /// <summary>
    /// Buffers the data received from a client, converting it into a contiguous chunk of data,
    /// therefore making it easier for the parser engine to be processed. 
    /// </summary>
    /// <remarks>
    /// Depending on how fast a client is able to upload request data,
    /// the server may need to wait for the whole request to be available.
    /// Additionally, keep alive connections will be held open by the client
    /// until there is a new request to be sent. The buffer implements request
    /// read timeouts by continuously reading data from the underlying network
    /// stream. If the read operation times out, the server will close the
    /// connection.
    /// </remarks>
    internal sealed class RequestBuffer : IDisposable
    {
        private ReadOnlySequence<byte>? _Data;

        #region Get-/Setters

        private PipeReader Reader { get; }

        private NetworkConfiguration Configuration { get; }

        private CancellationTokenSource? Cancellation { get; set; }

        internal ReadOnlySequence<byte> Data => _Data ?? new();

        internal bool ReadRequired => (_Data == null) || _Data.Value.IsEmpty;

        #endregion

        #region Initialization

        internal RequestBuffer(PipeReader reader, NetworkConfiguration configuration)
        {
            Reader = reader;
            Configuration = configuration;
        }

        #endregion

        #region Functionality

        internal async PooledValueTask<long?> Read(bool force = false)
        {
            if (ReadRequired || force)
            {
                if (_Data != null)
                {
                    Reader.AdvanceTo(_Data.Value.Start);
                }

                if (Cancellation is null)
                {
                    Cancellation = new();
                }

                try
                {
                    Cancellation.CancelAfter(Configuration.RequestReadTimeout);

                    _Data = (await Reader.ReadAsync(Cancellation.Token)).Buffer;

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

        internal void Commit(SequencePosition position)
        {
            _Data = Data.Slice(position);
        }

        internal void Commit(long bytes)
        {
            _Data = Data.Slice(bytes);
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
