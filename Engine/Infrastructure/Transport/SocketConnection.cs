using System;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace GenHTTP.Engine.Infrastructure.Transport
{

    internal sealed class SocketConnection
    {

        #region Get-/Setters

        internal Socket Socket { get; }

        internal ServicePipe Pipe { get; }

        private SocketReceiver Receiver { get; }

        private SocketSenderPool SenderPool { get; }

        public PipeWriter Input => Pipe.Application.Output;

        public PipeReader Output => Pipe.Application.Input;

        #endregion

        #region Initialization

        internal SocketConnection(Socket socket)
        {
            Socket = socket;

            Pipe = ServicePipe.Create();

            Receiver = new SocketReceiver(Pipe.Scheduler);

            SenderPool = new SocketSenderPool();
        }

        #endregion

        #region Functionality

        internal void Run()
        {
            var _1 = ReceiveAsync();
            var _2 = SendAsync();
        }

        private async Task ReceiveAsync()
        {
            try
            {
                while (true) // todo
                {
                    var waitResult = await Receiver.WaitForDataAsync(Socket);

                    if (waitResult.HasError)
                    {
                        break;
                    }

                    var buffer = Input.GetMemory(2048);

                    var readResult = await Receiver.ReceiveAsync(Socket, buffer);

                    if (readResult.HasError)
                    {
                        break;
                    }

                    var bytesRead = readResult.BytesTransferred;

                    if (bytesRead == 0)
                    {
                        break;
                    }

                    Input.Advance(bytesRead);

                    var result = await Input.FlushAsync();

                    if (result.IsCompleted || result.IsCanceled)
                    {
                        break;
                    }
                }
            }
            catch
            {

            }
        }

        private async Task SendAsync()
        {
            try
            {
                while (true)
                {
                    var result = await Output.ReadAsync();

                    if (result.IsCanceled)
                    {
                        break;
                    }

                    var buffer = result.Buffer;

                    if (!buffer.IsEmpty)
                    {
                        var sender = SenderPool.Rent();

                        try
                        {
                            var transferResult = await sender.SendAsync(Socket, buffer);

                            if (transferResult.HasError)
                            {
                                break;
                            }
                        }
                        finally
                        {
                            SenderPool.Return(sender);
                        }
                    }

                    Output.AdvanceTo(buffer.End);

                    if (result.IsCompleted)
                    {
                        break;
                    }
                }

                try
                {
                    Socket.Shutdown(SocketShutdown.Both);
                }
                catch
                {
                    // nop
                }
            }
            catch
            {
                Socket.Close(timeout: 0);
            }
            finally
            {
                Socket.Dispose();
            }
        }

        #endregion

    }

}
