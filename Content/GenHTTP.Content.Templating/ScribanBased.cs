using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

using Scriban;

namespace GenHTTP.Content.Templating
{
    
    public class ScribanBased<T>
    {

        #region Get-/Setters
        
        internal Template Template { get; }

        #endregion

        #region Initialization

        protected ScribanBased(string template)
        {
            Template = Template.Parse(template);
        }

        #endregion

        #region Functionality

        protected string Render(T viewModel)
        {
            return Template.Render(viewModel);
        }
        
        #endregion

    }

}
