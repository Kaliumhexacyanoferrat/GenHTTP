using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Modules.Templating
{
    
    /// <summary>
    /// Interface which needs to be implemented by all models
    /// used to render pages or templates.
    /// </summary>
    public interface IBaseModel
    {
        
        /// <summary>
        /// The request belonging to the current rendering call.
        /// </summary>
        IRequest Request { get; }
        
    }

}
