using System;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Strings
{

    public sealed class StringResourceBuilder : IResourceBuilder<StringResourceBuilder>
    {
        private string? _Content, _Name;

        private FlexibleContentType? _ContentType;

        private DateTime? _Modified;

        #region Functionality

        public StringResourceBuilder Content(string content)
        {
            _Content = content;
            return this;
        }

        public StringResourceBuilder Name(string name)
        {
            _Name = name;
            return this;
        }

        public StringResourceBuilder Type(FlexibleContentType contentType)
        {
            _ContentType = contentType;
            return this;
        }

        public StringResourceBuilder Modified(DateTime modified)
        {
            _Modified = modified;
            return this;
        }

        public IResource Build()
        {
            var content = _Content ?? throw new BuilderMissingPropertyException("content");

            return new StringResource(content, _Name, _ContentType, _Modified);
        }

        #endregion

    }

}
