using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
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

    public ValueTask PrepareAsync(IServer server) => Content.PrepareAsync(server);

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        if (request.Header.Query.ContainsKey(InspectInstruction))
        {
            var accepted = request.Header.Headers.GetEntry(KnownHeaders.Accept);

            var response = await Content.HandleAsync(request);

            var server = request.Server;

            var header = request.Header;

            var target = header.Target;

            var responseContent = response?.Content;

            var model = new
            {
                Server = new
                {
                    Version = server.Version,
                    Development = server.Development,
                    Handler = server.Handler.ToString(),
                    Endpoints = server.EndPoints.Select(e => new
                    {
                        Port = e.Port,
                        IPAddress = e.Address?.ToString(),
                        Secure = e.Secure,
                        RequestSource = e == request.EndPoint
                    })
                },
                Client = new
                {
                    Protocol = request.Client.Protocol,
                    IPAddress = request.Client.IPAddress?.ToString()
                },
                Request = new
                {
                    Protocol = header.Protocol.ToString(),
                    Method = header.Method.ToString(),
                    Path = header.Path.ToString(),
                    Target = new
                    {
                        Current = target.Current?.ToString(),
                        TrailingSlash = target.HasTrailingSlash,
                        Last = target.IsLast
                    },
                    Headers = GetList(header.Headers),
                    Query = GetList(header.Query)
                },
                Response = (response != null) ? new {
                    Status = response.Status,
                    Mode = response.Mode,
                    Headers = GetList(response.Headers),
                    Content = (responseContent != null) ? new
                    {
                        Length = responseContent.Length,
                        Type = responseContent.Type?.ToString(),
                        Checksum = await responseContent.CalculateChecksumAsync()
                    } : null
                } : null
            };

            var format = Serialization.GetSerialization(request, accepted);

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

    private static List<KeyValuePair<string, string>> GetList(IKeyValueList list)
    {
        var result = new List<KeyValuePair<string, string>>();

        for (var i = 0; i < list.Count; i++)
        {
            var entry = list[i];

            result.Add(new(entry.Key.ToString(), entry.Value.ToString()));
        }

        return result;
    }

    #endregion

}
