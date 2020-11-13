﻿using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.IO.Providers
{
    
    public class DownloadProvider : IHandler
    {
        private ContentInfo? _Info;

        #region Get-/Setters

        public IHandler Parent { get; }

        public IResource Resource { get; }

        public string? FileName { get; }

        private FlexibleContentType ContentType { get; }

        private ContentInfo Info
        {
            get
            {
                return _Info ??= ContentInfo.Create()
                                            .Title(FileName ?? "Download")
                                            .Build();
            }
        }

        #endregion

        #region Initialization

        public DownloadProvider(IHandler parent, IResource resourceProvider, string? fileName, FlexibleContentType? contentType)
        {
            Parent = parent;

            Resource = resourceProvider;

            FileName = fileName ?? Resource.Name;

            ContentType = contentType ?? Resource.ContentType ?? new FlexibleContentType(FileName?.GuessContentType() ?? Api.Protocol.ContentType.ApplicationForceDownload);
        }

        #endregion

        #region Functionality

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            if (!request.Target.Ended)
            {
                return null;
            }

            if (!request.HasType(RequestMethod.GET, RequestMethod.HEAD))
            {
                return (await this.GetMethodNotAllowedAsync(request).ConfigureAwait(false)).Build();
            }

            var response = request.Respond()
                                  .Content(Resource)
                                  .Type(ContentType);

            var fileName = FileName ?? Resource.Name;

            if (fileName is not null)
            {
                response.Header("Content-Disposition", $"attachment; filename=\"{fileName}\"");
            }
            else
            {
                response.Header("Content-Disposition", "attachment");
            }

            return response.Build();
        }

        public IEnumerable<ContentElement> GetContent(IRequest request) => this.GetContent(request, Info, ContentType);

        #endregion

    }

}
