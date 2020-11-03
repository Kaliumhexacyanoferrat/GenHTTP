using GenHTTP.Api.Content.IO;

namespace GenHTTP.Api.Content.Websites
{

    public class Style
    {

        #region Get-/Setters

        public string Name { get; }

        public IResource Provider { get; }

        #endregion

        #region Initialization

        public Style(string name, IResource provider)
        {
            Name = name;
            Provider = provider;
        }

        #endregion

    }

}
