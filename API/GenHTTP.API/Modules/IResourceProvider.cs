using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GenHTTP.Api.Modules
{

    public interface IResourceProvider
    {

        Stream GetResource();

        string GetResourceAsString();

    }

}
