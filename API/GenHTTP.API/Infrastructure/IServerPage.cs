using GenHTTP.Api.Compilation;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Api.Infrastructure
{

    public interface IServerPage : ITemplate
    {

        /// <summary>
        /// The title of the page.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// The content of the main box.
        /// </summary>
        string Value { get; set; }

    }

}
