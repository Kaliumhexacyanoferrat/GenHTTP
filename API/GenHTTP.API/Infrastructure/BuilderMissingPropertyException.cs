using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Api.Infrastructure
{

    public class BuilderMissingPropertyException : Exception
    {

        public BuilderMissingPropertyException(string property) : base($"Missing required property '{property}'")
        {

        }

    }

}
