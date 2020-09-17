﻿using System;
using System.Runtime.Serialization;

namespace GenHTTP.Api.Infrastructure
{

    /// <summary>
    /// Will be thrown, if the server cannot bind to the requested port for some reason.
    /// </summary>
    [Serializable]
    public class BindingException : Exception
    {

        public BindingException(string message, Exception inner) : base(message, inner)
        {

        }

        protected BindingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }

    }

}
