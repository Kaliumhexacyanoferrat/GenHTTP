using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Routing;

namespace GenHTTP.Api.Infrastructure
{

    public interface IServerBuilder : IBuilder<IServer>
    {

        #region Content

        IServerBuilder Router(IRouterBuilder routerBuilder);

        IServerBuilder Router(IRouter router);

        #endregion

        #region Infrastructure
        
        IServerBuilder Console();

        IServerBuilder Companion(IServerCompanion companion);

        IServerBuilder Extension(IBuilder<IServerExtension> extension);

        IServerBuilder Extension(IServerExtension extension);

        #endregion

        #region Compression

        IServerBuilder Compression(IBuilder<ICompressionAlgorithm> algorithm);

        IServerBuilder Compression(ICompressionAlgorithm algorithm);

        IServerBuilder Compression(bool enabled);

        #endregion

        #region Binding

        IServerBuilder Port(ushort port);

        IServerBuilder Backlog(ushort backlog);

        #endregion

        #region Network settings

        /// <summary>
        /// Specifies the period of time after which the server will
        /// assume the client connection timed out.
        /// </summary>
        IServerBuilder RequestReadTimeout(TimeSpan timeout);

        /// <summary>
        /// Requests smaller than this limit will be held in memory, while
        /// larger requests will be cached in a temporary file.
        /// </summary>
        IServerBuilder RequestMemoryLimit(uint limit);

        /// <summary>
        /// Size of the buffer that will be used to read or write large
        /// data streams (such as uploads or downloads).
        /// </summary>
        IServerBuilder TransferBufferSize(uint bufferSize);

        #endregion

    }
    
}
