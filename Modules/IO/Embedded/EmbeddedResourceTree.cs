using GenHTTP.Api.Content.IO;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace GenHTTP.Modules.IO.Embedded
{
    
    internal class EmbeddedResourceTree : EmbeddedResourceContainer, IResourceTree
    {

        internal EmbeddedResourceTree(Assembly source, string root) : base(source, root) { }

    }

}
