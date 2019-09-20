using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Modules.Core.Layouting;

namespace GenHTTP.Modules.Webservices
{
    
    /// <summary>
    /// Allows to easily add resources to layouts.
    /// </summary>
    public static class LayoutExtensions
    {

        /// <summary>
        /// Adds the given webservice resource to the layout, accessible using
        /// the specified path.
        /// </summary>
        /// <typeparam name="T">The type of the resource to be added</typeparam>
        /// <param name="path">The path the resource should be available at</param>
        public static LayoutBuilder Add<T>(this LayoutBuilder layout, string path) where T : new()
        {
            return layout.Add(path, Resource.From<T>().Build());
        }

        /// <summary>
        /// Adds the given webservice resource to the layout, accessible using
        /// the specified path.
        /// </summary>
        /// <param name="path">The path the resource should be available at</param>
        /// <param name="instance">The webservice resource instance</param>
        public static LayoutBuilder Add(this LayoutBuilder layout, string path, object instance)
        {
            return layout.Add(path, Resource.From(instance).Build());
        }

    }

}
