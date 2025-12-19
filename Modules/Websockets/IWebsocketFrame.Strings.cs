using System.Text;

namespace GenHTTP.Modules.Websockets;

public static class WebsocketFrameStringExtensions
{

    /// <summary>
    /// Reads the data provided by the websocket frame
    /// as a UTF-8 string.
    /// </summary>
    /// <returns>The UTF-8 encoded string data of the frame</returns>
    public static string DataAsString(this IWebsocketFrame frame) => Encoding.UTF8.GetString(frame.Data.ToArray()); // Can allocate twice

}
