using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Api.Modules.Websites
{
    
    public class ScriptReference
    {

        #region Get-/Setters

        public string Path { get; }

        public bool Async { get; }

        #endregion

        #region Functionality

        public ScriptReference(string path, bool asynchronous)
        {
            Path = path;
            Async = asynchronous;
        }

        #endregion

    }

}
