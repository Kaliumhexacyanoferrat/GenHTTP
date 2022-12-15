using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Ranges
{

    public class RangeSupportConcern : IConcern
    {
        private static readonly Regex _PATTERN = new(@"^\s*bytes\s*=\s*([0-9]*)-([0-9]*)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        #region Get-/Setters

        public IHandler Content { get; }

        public IHandler Parent { get; }

        #endregion

        #region Initialization

        public RangeSupportConcern(IHandler parent, Func<IHandler, IHandler> contentFactory)
        {
            Parent = parent;
            Content = contentFactory(this);
        }

        #endregion

        #region Functionality

        public ValueTask PrepareAsync() => ValueTask.CompletedTask;

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request) => Content.GetContentAsync(request);

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            if (request.Method == RequestMethod.GET || request.Method == RequestMethod.HEAD)
            {
                var response = await Content.HandleAsync(request);

                if (response != null)
                {
                    if (response.Status == ResponseStatus.OK)
                    {
                        var length = response.ContentLength;

                        if (length != null)
                        {
                            response["Accept-Ranges"] = "bytes";

                            if (request.Headers.TryGetValue("Range", out var requested))
                            {
                                var match = _PATTERN.Match(requested);

                                if (match.Success)
                                {
                                    var startString = match.Groups[1].Value;
                                    var endString = match.Groups[2].Value;

                                    var start = (string.IsNullOrEmpty(startString)) ? null : (ulong?)ulong.Parse(startString);
                                    var end = (string.IsNullOrEmpty(endString)) ? null : (ulong?)ulong.Parse(endString);

                                    if ((start != null) || (end != null))
                                    {
                                        if (end == null)
                                        {
                                            return GetRangeFromStart(request, response, length!.Value, start!.Value);
                                        }
                                        else if (start == null)
                                        {
                                            return GetRangeFromEnd(request, response, length!.Value, end!.Value);
                                        }
                                        else
                                        {
                                            return GetFullRange(request, response, length!.Value, start!.Value, end!.Value);
                                        }
                                    }
                                }
                                else
                                {
                                    return NotSatisfiable(request, length!.Value);
                                }
                            }
                        }
                    }
                }

                return response;
            }
            else
            {
                return await Content.HandleAsync(request);
            }
        }

        private static IResponse GetRangeFromStart(IRequest request, IResponse response, ulong length, ulong start)
        {
            if (start > length) return NotSatisfiable(request, length);

            return GetRange(response, start, length - 1, length);
        }

        private static IResponse GetRangeFromEnd(IRequest request, IResponse response, ulong length, ulong end)
        {
            if (end > length) return NotSatisfiable(request, length);

            return GetRange(response, length - end, length - 1, length);
        }

        private static IResponse GetFullRange(IRequest request, IResponse response, ulong length, ulong start, ulong end)
        {
            if (start > end || end >= length) return NotSatisfiable(request, length);

            return GetRange(response, start, end, length);
        }

        private static IResponse GetRange(IResponse response, ulong actualStart, ulong actualEnd, ulong totalLength)
        {
            response.Status = new(ResponseStatus.PartialContent);

            response["Content-Range"] = $"bytes {actualStart}-{actualEnd}/{totalLength}";

            response.Content = new RangedContent(response.Content!, actualStart, actualEnd);
            response.ContentLength = actualEnd - actualStart + 1;

            return response;
        }

        private static IResponse NotSatisfiable(IRequest request, ulong totalLength)
        {
            var content = Resource.FromString($"Requested length cannot be satisfied (available = {totalLength} bytes)")
                                  .Build();

            return request.Respond()
                          .Status(ResponseStatus.RequestedRangeNotSatisfiable)
                          .Header("Content-Range", $"bytes */{totalLength}")
                          .Content(content)
                          .Build();
        }

        #endregion

    }

}
