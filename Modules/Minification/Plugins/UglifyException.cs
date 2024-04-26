using System;
using System.Collections.Generic;

using NUglify;

namespace GenHTTP.Modules.Minification.Plugins
{

    public sealed class UglifyException : Exception
    {

        public IReadOnlyList<UglifyError> Errors { get; }

        public UglifyException(string message, IReadOnlyList<UglifyError> errors) : base(message)
        {
            Errors = errors;
        }

    }

}
