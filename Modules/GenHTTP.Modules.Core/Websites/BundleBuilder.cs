﻿using System.Collections.Generic;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.Websites
{

    public class BundleBuilder : IHandlerBuilder
    {
        private readonly List<IResourceProvider> _Items = new List<IResourceProvider>();

        private FlexibleContentType _ContentType = new FlexibleContentType(Api.Protocol.ContentType.ApplicationForceDownload);

        #region Functionality

        public BundleBuilder Add(IResourceProvider resource)
        {
            _Items.Add(resource);
            return this;
        }

        public BundleBuilder ContentType(ContentType type) => ContentType(new FlexibleContentType(type));

        public BundleBuilder ContentType(FlexibleContentType type)
        {
            _ContentType = type;
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            return new BundleProvider(parent, _Items, _ContentType);
        }

        #endregion

    }

}
