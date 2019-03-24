using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GenHTTP.Api.Content.Pages
{

    public interface IContentPage : IContent
    {

        string Title { get; set; }

        string Content { get; set; }
        
    }

}
