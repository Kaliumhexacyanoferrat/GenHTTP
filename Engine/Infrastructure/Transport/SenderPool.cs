using System;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Threading;

namespace GenHTTP.Engine.Infrastructure.Transport
{

    internal sealed class SocketSenderPool : IDisposable
    {
        private const int MaxQueueSize = 1024;

        private readonly ConcurrentQueue<SocketSender> _queue = new();

        private readonly PipeScheduler _scheduler;

        private int _count;
        private bool _disposed;

        #region Get-/Setters

        public PipeScheduler Scheduler => _scheduler;

        #endregion

        #region Initialization

        public SocketSenderPool()
        {
            _scheduler = PipeScheduler.Inline;
        }

        #endregion

        #region Functionality

        public SocketSender Rent()
        {
            if (_queue.TryDequeue(out var sender))
            {
                Interlocked.Decrement(ref _count);
                return sender;
            }
            return new SocketSender(_scheduler);
        }

        public void Return(SocketSender sender)
        {
            // This counting isn't accurate, but it's good enough for what we need to avoid using _queue.Count which could be expensive
            if (_disposed || Interlocked.Increment(ref _count) > MaxQueueSize)
            {
                Interlocked.Decrement(ref _count);
                sender.Dispose();
                return;
            }

            sender.Reset();
            _queue.Enqueue(sender);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                while (_queue.TryDequeue(out var sender))
                {
                    sender.Dispose();
                }
            }
        }

        #endregion

    }

}
