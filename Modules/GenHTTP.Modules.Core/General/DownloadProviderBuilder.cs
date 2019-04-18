using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.General
{

    public class DownloadProviderBuilder : ContentBuilderBase
    {
        private IResourceProvider? _ResourceProvider;
        private ContentType? _ContentType;
        
        #region Functionality

        public DownloadProviderBuilder Resource(IResourceProvider resource)
        {
            _ResourceProvider = resource;
            return this;
        }

        public DownloadProviderBuilder Type(ContentType contentType)
        {
            _ContentType = contentType;
            return this;
        }

        public override IContentProvider Build()
        {
            if (_ResourceProvider == null)
            {
                throw new BuilderMissingPropertyException("Resource Provider");
            }

            if (_ContentType == null)
            {
                throw new BuilderMissingPropertyException("Content Type");
            }

            return new DownloadProvider(_ResourceProvider, (ContentType)_ContentType, _Modification);
        }

        #endregion

    }

}
