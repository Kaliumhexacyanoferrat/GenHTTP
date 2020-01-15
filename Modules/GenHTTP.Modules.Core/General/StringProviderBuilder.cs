﻿using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.General
{

    public class StringProviderBuilder : ContentBuilderBase
    {
        private string? _Data;
        private ContentType? _ContentType;

        #region Functionality

        public StringProviderBuilder Data(string data)
        {
            _Data = data;
            return this;
        }

        public StringProviderBuilder Type(ContentType type)
        {
            _ContentType = type;
            return this;
        }

        public override IContentProvider Build()
        {
            if (_Data == null)
            {
                throw new BuilderMissingPropertyException("Data");
            }

            if (_ContentType == null)
            {
                throw new BuilderMissingPropertyException("Content Type");
            }

            return new StringProvider(_Data, new FlexibleContentType((ContentType)_ContentType), _Modification);
        }

        #endregion

    }

}
