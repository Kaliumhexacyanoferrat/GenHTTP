using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Conversion.Serializers;
using GenHTTP.Modules.Conversion.Serializers.Yaml;

using Strings = GenHTTP.Modules.IO.Strings;

namespace GenHTTP.Modules.Inspection.Concern;

public sealed class InspectionConcern : IConcern
{

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
        if (request.Query.ContainsKey("inspect"))
        {
            using var content = await Content.HandleAsync(request);

            var server = request.Server;

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
                        IPAddress = e.Addresses?.Select(a => a.ToString()),
                        Secure = e.Secure,
                        RequestSource = e == request.EndPoint
                    })
                },
                Client = new
                {
                    Protocol = request.Client.Protocol,
                    IPAddress = request.Client.IPAddress.ToString(),
                    Host = request.Client.Host
                },
                LocalClient = (request.Client != request.LocalClient) ? new
                {
                    Protocol = request.LocalClient.Protocol,
                    IPAddress = request.LocalClient.IPAddress.ToString(),
                    Host = request.LocalClient.Host
                } : null,
                Request = new
                {
                    ProtocolType = request.ProtocolType,
                    Method = request.Method.RawMethod,
                    Path = request.Target.Path.ToString(),
                    Headers = request.Headers,
                    Query = request.Query,
                    Cookies = request.Cookies,
                    Content = (request.Content != null) ? new
                    {
                        Type = request.ContentType,
                        Body = request.Content.ToString()
                    } : null,
                    Forwardings = request.Forwardings,
                    Properties = request.Properties
                },
                Response = (content != null) ? new {
                    Status = content.Status.RawStatus,
                    Upgraded = content.Upgraded,
                    Expires = content.Expires,
                    Modified = content.Modified,
                    Headers = content.Headers,
                    Cookies = content.Cookies,
                    Content = new
                    {
                        Type = content.ContentType?.RawType,
                        Body = content.Content?.ToString(),
                        Length = content.Content?.Length,
                        Encoding = content.ContentEncoding
                    }
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

            var serializedModel = await format.SerializeAsync(request, model);

            if (format is YamlFormat)
            {
                // quirk: browsers do not display application/yaml
                serializedModel.Type(ContentType.TextYaml);
            }

            return serializedModel.Build();
        }

        return await Content.HandleAsync(request);
    }

    #endregion

}
