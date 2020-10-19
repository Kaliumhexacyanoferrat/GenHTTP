using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.IO.Providers
{

    public class StringDataProviderBuilder : IBuilder<IResource>
    {
        private string? _Content;

        #region Functionality

        public StringDataProviderBuilder Content(string content)
        {
            _Content = content;
            return this;
        }

        public IResource Build()
        {
            if (_Content == null)
            {
                throw new BuilderMissingPropertyException("Content");
            }

            return new StringDataProvider(_Content);
        }

        #endregion

    }

}
