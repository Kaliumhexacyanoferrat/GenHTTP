using System;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Content.IO
{

    /// <summary>
    /// When implemented by builders providing resource instances,
    /// this interface allows to configure common properties of
    /// resources in an unified way.
    /// </summary>
    public interface IResourceBuilder<out T> : IBuilder<IResource> where T : IResourceBuilder<T>
    {

        /// <summary>
        /// Sets the name of the resource.
        /// </summary>
        /// <param name="name">The name of the resource</param>
        T Name(string name);

        /// <summary>
        /// Sets the content type of the resource.
        /// </summary>
        /// <param name="contentType">The content type of the resource</param>
        T Type(FlexibleContentType contentType);

        /// <summary>
        /// Sets the modification date and time of the resource.
        /// </summary>
        /// <param name="modified">The modification date and time of the resource</param>
        T Modified(DateTime modified);

    }

    public static class IResourceMetaDataBuilderExtensions
    {

        /// <summary>
        /// Sets the content type of the resource.
        /// </summary>
        /// <param name="contentType">The content type of the resource</param>
        public static T Type<T>(this IResourceBuilder<T> builder, ContentType contentType) where T : IResourceBuilder<T> => builder.Type(FlexibleContentType.Get(contentType));

    }

}
