using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using GenHTTP.Api.Modules;

namespace GenHTTP.Modules.Core.Resource
{

    public class StringDataProvider : IResourceProvider
    {

        #region Get-/Setters

        public string Content { get; }

        #endregion

        #region Initialization

        public StringDataProvider(string content)
        {
            Content = content;
        }

        #endregion

        #region Functionality

        public Stream GetResource()
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(Content));
        }
        
        #endregion

    }

}
