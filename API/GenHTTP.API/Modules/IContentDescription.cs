using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Modules
{
    
    public interface IContentDescription
    {
        
        string? Title { get; }

        FlexibleContentType? ContentType { get; }

    }

}
