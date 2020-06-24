namespace GenHTTP.Api.Infrastructure
{

    /// <summary>
    /// Allows applications to manage the lifecycle of a server instance.
    /// </summary>
    public interface IServerHost : IServerBuilder<IServerHost>
    {

        /// <summary>
        /// The server instance maintained by the host, if started.
        /// </summary>
        IServer? Instance { get; }

        /// <summary>
        /// Builds a server instance from the current configuration
        /// and starts it.
        /// </summary>
        IServerHost Start();

        /// <summary>
        /// Stops the currently running server instance, if any.
        /// </summary>
        IServerHost Stop();

        /// <summary>
        /// Stops the currently running server instance and starts
        /// a new one.
        /// </summary>
        IServerHost Restart();

        /// <summary>
        /// Builds a server instance from the current configuration
        /// and keeps it running until the application process exits.
        /// </summary>
        /// <remarks>
        /// Convenience method that can be used as a one-liner by
        /// console or docker based applications.
        /// </remarks>
        /// <returns>The return code to be passed to the operating system</returns>
        int Run();

    }

}
