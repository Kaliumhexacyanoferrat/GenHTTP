using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Api.Infrastructure
{
    
    public interface ILog
    {

        void WriteLine(string message);

        void WriteLineColored(string message, ConsoleColor color);

        void WriteTimestamp();

    }

}
