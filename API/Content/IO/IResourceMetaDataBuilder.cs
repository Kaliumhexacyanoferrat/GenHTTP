using System;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Content.IO
{

    public interface IResourceMetaDataBuilder<out T>
    {

        T Name(string name);

        // ToDo: does not work?
        // ToDo: replace all wrong places as soon as it works
        T Type(ContentType contentType) => Type(new FlexibleContentType(contentType));

        T Type(FlexibleContentType contentType);

        T Modified(DateTime modified);

    }

}
