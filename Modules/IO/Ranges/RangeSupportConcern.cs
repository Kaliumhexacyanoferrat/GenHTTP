using System.Text.RegularExpressions;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.IO.Ranges;

public partial class RangeSupportConcern : IConcern
{
    private static readonly Regex Pattern = CreatePattern();
    
    private static readonly ReadOnlyMemory<byte> AcceptRangesHeader = "Accept-Ranges"u8.ToArray();
    
    private static readonly ReadOnlyMemory<byte> BytesValue = "bytes"u8.ToArray();

    #region Get-/Setters

    public IHandler Content { get; }

    #endregion

    #region Initialization

    public RangeSupportConcern(IHandler content)
    {
        Content = content;
    }

    [GeneratedRegex(@"^\s*bytes\s*=\s*([0-9]*)-([0-9]*)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex CreatePattern();

    #endregion

    #region Functionality

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var header = request.Header;
        
        if (header.Method == RequestMethod.Get || header.Method == RequestMethod.Head)
        {
            var response = await Content.HandleAsync(request);

            if (response != null)
            {
                if (response.Status == ResponseStatus.Ok)
                {
                    var content = response.Content;

                    if (content != null)
                    {
                        var length = content.Length;

                        if (length != null)
                        {
                            response.Rebuild().Header(AcceptRangesHeader, BytesValue);

                            var requested = header.Headers.GetEntry("Range");
                            
                            if (requested != null)
                            {
                                var match = Pattern.Match(requested);

                                if (match.Success)
                                {
                                    var startString = match.Groups[1].Value;
                                    var endString = match.Groups[2].Value;

                                    var start = string.IsNullOrEmpty(startString) ? null : (ulong?)ulong.Parse(startString);
                                    var end = string.IsNullOrEmpty(endString) ? null : (ulong?)ulong.Parse(endString);

                                    if (start != null || end != null)
                                    {
                                        if (end == null)
                                        {
                                            return GetRangeFromStart(request, response, length.Value, start!.Value);
                                        }
                                        if (start == null)
                                        {
                                            return GetRangeFromEnd(request, response, length.Value, end.Value);
                                        }
                                        return GetFullRange(request, response, length.Value, start.Value, end.Value);
                                    }
                                }
                                else
                                {
                                    return NotSatisfiable(request, length.Value);
                                }
                            }
                        }
                    }
                }
            }

            return response;
        }       


        return await Content.HandleAsync(request);
    }

    private static IResponse GetRangeFromStart(IRequest request, IResponse response, ulong length, ulong start)
    {
        if (start > length)
        {
            return NotSatisfiable(request, length);
        }

        return GetRange(response, start, length - 1, length);
    }

    private static IResponse GetRangeFromEnd(IRequest request, IResponse response, ulong length, ulong end)
    {
        if (end > length)
        {
            return NotSatisfiable(request, length);
        }

        return GetRange(response, length - end, length - 1, length);
    }

    private static IResponse GetFullRange(IRequest request, IResponse response, ulong length, ulong start, ulong end)
    {
        if (start > end || end >= length)
        {
            return NotSatisfiable(request, length);
        }

        return GetRange(response, start, end, length);
    }

    private static IResponse GetRange(IResponse response, ulong actualStart, ulong actualEnd, ulong totalLength)
    {
        response.Rebuild()
                .Status(ResponseStatus.PartialContent)
                .Header("Content-Range", $"bytes {actualStart}-{actualEnd}/{totalLength}")
                .Content(new RangedContent(response.Content!, actualStart, actualEnd));

        return response;
    }

    private static IResponse NotSatisfiable(IRequest request, ulong totalLength)
    {
        var content = Resource.FromString($"Requested length cannot be satisfied (available = {totalLength} bytes)").Build();

        return request.Respond()
                      .Status(ResponseStatus.RequestedRangeNotSatisfiable)
                      .Header("Content-Range", $"bytes */{totalLength}")
                      .Content(new ResourceContent(content, ContentType.TextPlain))
                      .Build();
    }

    #endregion

}
