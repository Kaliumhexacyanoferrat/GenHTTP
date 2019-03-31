using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GenHTTP.Api.Infrastructure
{
    
    public interface ICompressionAlgorithm
    {
        
        string Name { get; }

        Priority Priority { get; }

        Stream Compress(Stream content);

    }

}
