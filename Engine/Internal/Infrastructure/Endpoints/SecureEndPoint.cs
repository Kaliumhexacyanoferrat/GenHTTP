using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Internal.Protocol;
using GenHTTP.Engine.Internal.Utilities;
using GenHTTP.Engine.Shared.Infrastructure;

namespace GenHTTP.Engine.Internal.Infrastructure.Endpoints;

internal sealed class SecureEndPoint : EndPoint
{

    #region Get-/Setters

    internal SecurityConfiguration Options { get; }

    public override bool Secure => true;

    private SslServerAuthenticationOptions AuthenticationOptions { get; }

    #endregion

    #region Initialization

    internal SecureEndPoint(IServer server, IPAddress? address, ushort port, SecurityConfiguration options, NetworkConfiguration configuration)
        : base(server, address, port, configuration)
    {
        Options = options;

        AuthenticationOptions = new SslServerAuthenticationOptions
        {
            EnabledSslProtocols = Options.Protocols,
            AllowRenegotiation = true,
            ApplicationProtocols = new List<SslApplicationProtocol>
            {
                SslApplicationProtocol.Http11
            },
            EncryptionPolicy = EncryptionPolicy.RequireEncryption,
            ServerCertificateSelectionCallback = SelectCertificate,
            ClientCertificateRequired = Options.CertificateValidator?.RequireCertificate ?? false,
            CertificateRevocationCheckMode = Options.CertificateValidator?.RevocationCheck ?? X509RevocationMode.NoCheck,
            RemoteCertificateValidationCallback = ValidateClient
        };
    }

    #endregion

    #region Functionality

    protected override async ValueTask Accept(Socket client)
    {
        var stream = await TryAuthenticate(client);

        if (stream is not null)
        {
            await Handle(client, new PoolBufferedStream(stream, Configuration.TransferBufferSize), stream.RemoteCertificate);
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

    private X509Certificate2 SelectCertificate(object _, string? hostName)
    {
        var certificate = Options.CertificateProvider.Provide(hostName);

        if (certificate is null)
        {
            throw new InvalidOperationException($"The provider did not return a certificate to be used for host '{hostName}'");
        }

        return certificate;
    }

    private bool ValidateClient(object _, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslpolicyerrors)
        => Options.CertificateValidator?.Validate(certificate, chain, sslpolicyerrors) ?? true;

    #endregion

}
