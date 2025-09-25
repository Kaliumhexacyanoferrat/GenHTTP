using Fleck;

namespace GenHTTP.Modules.Websockets.Handler;

/// <summary>
/// Flecks uses synchronous callbacks for integration of user
/// logic, but provides asynchronous methods for interacting
/// with the websocket connection, which is counter-intuitive
/// and leads to errors. GenHTTP´s integration forces asynchronous
/// callbacks to be supplied by the user which requires us to dispatch
/// them on the synchronous callbacks invoked by Fleck.
/// </summary>
public static class WebsocketDispatcher
{
    
    /// <summary>
    /// Schedules the given piece of work on a background
    /// thread and logs any error using the regular Fleck
    /// logging mechanism.
    /// </summary>
    /// <param name="work">The actual piece of work to be executed</param>
    public static void Schedule(Func<Task> work)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await work().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                FleckLog.Error("Failed to run asynchronous event handler.", e);
            }
        });
    }
    
}
