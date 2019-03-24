using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GenHTTP.Api.Content
{
    
    public interface IContent
    {

        Stream GetStream();

    }

}
