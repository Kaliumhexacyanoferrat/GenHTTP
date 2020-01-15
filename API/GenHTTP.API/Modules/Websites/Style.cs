namespace GenHTTP.Api.Modules.Websites
{

    public class Style
    {

        #region Get-/Setters

        public string Name { get; }

        public IResourceProvider Provider { get; }

        #endregion

        #region Initialization

        public Style(string name, IResourceProvider provider)
        {
            Name = name;
            Provider = provider;
        }

        #endregion

    }

}
