using GenHTTP.Api.Content.IO;

namespace GenHTTP.Api.Content.Websites
{

    public class Script
    {

        #region Get-/Setters

        public string Name { get; }

        public bool Async { get; }

        public IResource Provider { get; }

        #endregion

        #region Initialization

        public Script(string name, bool asynchronous, IResource provider)
        {
            Name = name;
            Async = asynchronous;
            Provider = provider;
        }

        #endregion

    }

}
