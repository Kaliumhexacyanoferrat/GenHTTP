namespace GenHTTP.Api.Modules.Websites
{

    public class Script
    {

        #region Get-/Setters

        public string Name { get; }

        public bool Async { get; }

        public IResourceProvider Provider { get; }

        #endregion

        #region Initialization

        public Script(string name, bool asynchronous, IResourceProvider provider)
        {
            Name = name;
            Async = asynchronous;
            Provider = provider;
        }

        #endregion

    }

}
