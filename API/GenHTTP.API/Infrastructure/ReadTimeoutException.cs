using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Api.Infrastructure
{

    public class ReadTimeoutException : NetworkException
    {

        public ReadTimeoutException() : base("The client didn't send data in time")
        {

        }

    }

}
