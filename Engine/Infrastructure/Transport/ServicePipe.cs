using System.Buffers;
using System.IO.Pipelines;

namespace GenHTTP.Engine.Infrastructure.Transport
{

    internal readonly struct ServicePipe
    {
        private static readonly MemoryPool<byte> MEMORY_POOL = MemoryPool<byte>.Shared;

        #region Get-/Setters

        internal IDuplexPipe Transport { get; }

        internal IDuplexPipe Application { get; }

        internal IOQueue Scheduler { get; }

        #endregion

        #region Initialization

        internal static ServicePipe Create()
        {
            var applicationScheduler = PipeScheduler.ThreadPool;

            var transportScheduler = new IOQueue();

            var inputOptions = new PipeOptions(MEMORY_POOL, applicationScheduler, transportScheduler, useSynchronizationContext: false);

            var outputOptions = new PipeOptions(MEMORY_POOL, transportScheduler, applicationScheduler, useSynchronizationContext: false);

            var input = new Pipe(inputOptions);
            var output = new Pipe(outputOptions);

            var transport = new DuplexPipe(output.Reader, input.Writer);
            var application = new DuplexPipe(input.Reader, output.Writer);

            return new ServicePipe(application, transport, transportScheduler);
        }

        private ServicePipe(IDuplexPipe transport, IDuplexPipe application, IOQueue scheduler)
        {
            Transport = transport;
            Application = application;

            Scheduler = scheduler;
        }

        #endregion

    }

}
