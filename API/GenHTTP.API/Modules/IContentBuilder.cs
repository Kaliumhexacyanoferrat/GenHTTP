using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Api.Modules
{

    /// <summary>
    /// A builder which will provide an <see cref="IContentProvider"/>.
    /// </summary>
    public interface IContentBuilder : IBuilder<IContentProvider>
    {

    }

}
