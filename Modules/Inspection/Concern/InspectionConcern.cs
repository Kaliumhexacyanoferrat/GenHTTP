using System.Text;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Conversion.Serializers;
using Strings = GenHTTP.Modules.IO.Strings;

namespace GenHTTP.Modules.Inspection.Concern;

public sealed class InspectionConcern : IConcern
{
    private static readonly ByteString InspectInstruction = new("inspect");

    #region Get-/Setters

    public IHandler Content { get; }

    public SerializationRegistry Serialization { get; }

    #endregion

    #region Initialization

    public InspectionConcern(IHandler content, SerializationRegistry serialization)
    {
        Content = content;
        Serialization = serialization;
    }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync() => Content.PrepareAsync();

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        if (request.Header.Query.ContainsKey(InspectInstruction))
        {
            var response = await Content.HandleAsync(request);

            var server = request.Server;

            var header = request.Header;
            
            var model = new
            {
                Server = new
                {
                    Version = server.Version,
                    Development = server.Development,
                    Handler = server.Handler.ToString(),
                    Companion = server.Companion?.ToString(),
                    Endpoints = server.EndPoints.Select(e => new
                    {
                        Port = e.Port,
                        IPAddress = e.Address?.ToString(),
                        Secure = e.Secure,
                        RequestSource = e == request.EndPoint
                    })
                },
                // todo
                /*Client = new
                {
                    Protocol = request.Client.Protocol,
                    IPAddress = request.Client.IPAddress?.ToString(),
                    Host = request.Client.Host
                },
                LocalClient = (request.Client != request.LocalClient) ? new
                {
                    Protocol = request.LocalClient.Protocol,
                    IPAddress = request.LocalClient.IPAddress?.ToString(),
                    Host = request.LocalClient.Host
                } : null,*/
                Request = new
                {
                    Protocol = GetString(header.Protocol.Bytes),
                    Method = GetString(header.Method.Bytes),
                    Path = GetString(header.Path),
                    Target = new
                    {
                        Current = GetString(header.Target.Current?.Bytes),
                        TrailingSlash = header.Target.HasTrailingSlash,
                        Last = header.Target.IsLast
                    },
                    // todo: content
                },
                Response = (response != null) ? new {
                    // todo
                } : null
            };

            var format = Serialization.GetSerialization(request);

            if (format == null)
            {
                return request.Respond()
                              .Status(ResponseStatus.UnsupportedMediaType)
                              .Content(new Strings.StringContent("Unable to find serializer for requested format"))
                              .Build();
            }

            return (await format.SerializeAsync(request, model)).Build();
        }

        return await Content.HandleAsync(request);
    }

    private static string GetString(ReadOnlyMemory<byte>? value)
        => (value != null) ? Encoding.ASCII.GetString(value.Value.Span) : string.Empty;

    #endregion

}
