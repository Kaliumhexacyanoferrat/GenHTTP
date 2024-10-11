using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Engine.Protocol;
using GenHTTP.Engine.Utilities;

using PooledAwait;

namespace GenHTTP.Engine.Infrastructure.Endpoints
{

    internal sealed class SecureEndPoint : EndPoint
    {

        #region Get-/Setters

        internal SecurityConfiguration Options { get; }

        public override bool Secure => true;

        private SslServerAuthenticationOptions AuthenticationOptions { get; }

        #endregion

        #region Initialization

        internal SecureEndPoint(IServer server, IPEndPoint endPoint, SecurityConfiguration options, NetworkConfiguration configuration)
            : base(server, endPoint, configuration)
        {
            Options = options;

            AuthenticationOptions = new()
            {
                EnabledSslProtocols = Options.Protocols,
                ClientCertificateRequired = false,
                AllowRenegotiation = true,
                ApplicationProtocols = new() { SslApplicationProtocol.Http11 },
                CertificateRevocationCheckMode = X509RevocationMode.NoCheck, // no support for client certificates yet
                EncryptionPolicy = EncryptionPolicy.RequireEncryption,
                ServerCertificateSelectionCallback = SelectCertificate
            };
        }

        #endregion

        #region Functionality

        protected override async PooledValueTask Accept(Socket client)
        {
            var stream = await TryAuthenticate(client);

            if (stream is not null)
            {
                await Handle(client, new PoolBufferedStream(stream, Configuration.TransferBufferSize));
            }
            else
            {
                try
                {
                    client.Close();
                    client.Dispose();
                }
                catch (Exception e)
                {
                    Server.Companion?.OnServerError(ServerErrorScope.ClientConnection, client.GetAddress(), e);
                }
            }
        }

        private async ValueTask<SslStream?> TryAuthenticate(Socket client)
        {
            try
            {
                var stream = new SslStream(new NetworkStream(client), false);

                await stream.AuthenticateAsServerAsync(AuthenticationOptions, CancellationToken.None);

                return stream;
            }
            catch (Exception e)
            {
                Server.Companion?.OnServerError(ServerErrorScope.Security, client.GetAddress(), e);

                return null;
            }
        }

        private X509Certificate2 SelectCertificate(object sender, string? hostName)
        {
            var certificate = Options.Certificate.Provide(hostName);

            if (certificate is null)
            {
                throw new InvalidOperationException($"The provider did not return a certificate to be used for host '{hostName}'");
            }

            return certificate;
        }

        #endregion

    }

}
