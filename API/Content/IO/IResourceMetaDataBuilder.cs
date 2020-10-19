using GenHTTP.Api.Protocol;
using System;

namespace GenHTTP.Api.Content.IO
{

    public interface IResourceMetaDataBuilder<out T>
    {

        T Name(string name);

        T Type(ContentType contentType) => Type(new FlexibleContentType(contentType));

        T Type(FlexibleContentType contentType);

        T Modified(DateTime modified);

    }

}
