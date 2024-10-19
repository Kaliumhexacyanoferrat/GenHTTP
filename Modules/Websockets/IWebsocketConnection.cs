using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Websockets
{

    public interface IWebsocketConnection
    {

        IRequest Request { get; }

        bool IsAvailable { get; }

        Task Send(string message);

        Task Send(byte[] message);

        Task SendPing(byte[] message);

        Task SendPong(byte[] message);

        void Close();

        void Close(int code);

    }

}
