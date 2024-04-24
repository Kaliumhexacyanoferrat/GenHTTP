using System.IO.Pipelines;

namespace GenHTTP.Engine.Infrastructure.Transport
{

    internal sealed class DuplexPipe : IDuplexPipe
    {

        #region Get-/Setters

        public PipeReader Input { get; }

        public PipeWriter Output { get; }

        #endregion

        #region Initialization

        public DuplexPipe(PipeReader reader, PipeWriter writer)
        {
            Input = reader;
            Output = writer;
        }

        #endregion

    }

}
