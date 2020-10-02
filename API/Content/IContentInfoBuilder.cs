using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Api.Content
{
    
    public interface IContentInfoBuilder<T>
    {

        T Title(string title);

        T Description(string description);

    }

}
