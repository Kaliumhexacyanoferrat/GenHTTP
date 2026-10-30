using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Playground.Samples;

// imported here as the Websocket.Client package (referenced by the test project) declares a root namespace named "Websocket"
using GenHTTP.Modules.Websockets;

public static class WebsocketSample
{

    public static IHandlerBuilder Create()
    {
        /*
         *
         * A chat room: every message a client sends over the websocket is
         * pushed to all connected clients.
         *
         * Open http://localhost:8080/websockets/ in two browser windows and start chatting.
         *
         * See https://genhttp.org/documentation/content/frameworks/websockets/
         *
         */

        var chat = Websocket.Reactive()
                            .Handler(new ChatHandler());

        var client = Content.From(Resource.FromString(ClientPage).Type(ContentType.TextHtml));

        return Layout.Create()
                     .Index(client) // GET http://localhost:8080/websockets/
                     .Add("chat", chat); // ws://localhost:8080/websockets/chat
    }

    public record ChatMessage(string Author, string Text, DateTime? Sent = null);

    public class ChatHandler : IReactiveHandler
    {
        private readonly List<IReactiveConnection> _clients = [];

        // connections are not thread-safe, so we never write to them concurrently
        private readonly SemaphoreSlim _sync = new(1, 1);

        public async ValueTask OnConnected(IReactiveConnection connection)
        {
            await _sync.WaitAsync();

            try
            {
                _clients.Add(connection);
            }
            finally
            {
                _sync.Release();
            }
        }

        public async ValueTask OnMessage(IReactiveConnection connection, IWebsocketFrame message)
        {
            // JSON sent by the client is deserialized into our record
            var received = await message.ReadPayloadAsync<ChatMessage>();

            var toSend = received with { Sent = DateTime.UtcNow };

            await _sync.WaitAsync();

            try
            {
                foreach (var client in _clients)
                {
                    // ... and serialized to JSON again
                    await client.WritePayloadAsync(toSend);
                }
            }
            finally
            {
                _sync.Release();
            }
        }

        public async ValueTask OnClose(IReactiveConnection connection, IWebsocketFrame message)
        {
            await _sync.WaitAsync();

            try
            {
                _clients.Remove(connection);
            }
            finally
            {
                _sync.Release();
            }

            await connection.CloseAsync();
        }

    }

    private const string ClientPage =
        """
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="utf-8">
            <title>Chat</title>
        </head>
        <body>
            <form id="form">
                <input id="author" placeholder="Your name" required>
                <input id="text" placeholder="Your message" required autofocus>
                <button>Send</button>
            </form>
            <ul id="messages"></ul>
            <script>
                const socket = new WebSocket(new URL('chat', location.href.replace(/^http/, 'ws')));

                socket.onmessage = (event) => {
                    const message = JSON.parse(event.data);

                    const item = document.createElement('li');
                    item.textContent = `[${new Date(message.sent).toLocaleTimeString()}] ${message.author}: ${message.text}`;

                    document.getElementById('messages').prepend(item);
                };

                document.getElementById('form').onsubmit = (event) => {
                    event.preventDefault();

                    const text = document.getElementById('text');

                    socket.send(JSON.stringify({ author: document.getElementById('author').value, text: text.value }));

                    text.value = '';
                };
            </script>
        </body>
        </html>
        """;

}
