using GenHTTP.Api.Content;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Api.Protocol
{

    public interface IConcern : IHandler
    {

        IHandler Content { get; }

    }

}
