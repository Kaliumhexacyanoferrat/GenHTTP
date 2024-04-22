using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenHTTP.Engine.Protocol.Parser
{

    internal enum ScannerMode
    {
        Words,
        Path,
        HeaderKey,
        HeaderValue
    }

}
