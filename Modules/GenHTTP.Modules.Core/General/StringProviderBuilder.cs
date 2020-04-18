using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using System.Collections.Generic;

namespace GenHTTP.Modules.Core.General
{

    public class StringProviderBuilder : IHandlerBuilder<StringProviderBuilder>
    {
        private string? _Data;
        private ContentType? _ContentType;

        private readonly List<IConcernBuilder> _Concerns = new List<IConcernBuilder>();

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

        public StringProviderBuilder Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            if (_Data == null)
            {
                throw new BuilderMissingPropertyException("Data");
            }

            if (_ContentType == null)
            {
                throw new BuilderMissingPropertyException("Content Type");
            }

            return Concerns.Chain(parent, _Concerns, (p) => new StringProvider(p, _Data, new FlexibleContentType((ContentType)_ContentType)));
        }

        #endregion

    }

}
