﻿using System.Diagnostics.CodeAnalysis;

using GenHTTP.Modules.Layouting.Provider;

namespace GenHTTP.Modules.Webservices
{

    /// <summary>
    /// Extensions to simplify handling of service resources.
    /// </summary>
    public static class Extensions
    {

        /// <summary>
        /// Adds the given webservice resource to the layout, accessible using
        /// the specified path.
        /// </summary>
        /// <typeparam name="T">The type of the resource to be added</typeparam>
        /// <param name="path">The path the resource should be available at</param>
        public static LayoutBuilder AddService<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this LayoutBuilder layout, string path) where T : new()
        {
            return layout.Add(path, ServiceResource.From<T>());
        }

        /// <summary>
        /// Adds the given webservice resource to the layout, accessible using
        /// the specified path.
        /// </summary>
        /// <param name="path">The path the resource should be available at</param>
        /// <param name="instance">The webservice resource instance</param>
        public static LayoutBuilder AddService(this LayoutBuilder layout, string path, object instance)
        {
            return layout.Add(path, ServiceResource.From(instance));
        }

    }

}
