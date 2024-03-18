using System.Collections.Generic;

using GenHTTP.Api.Content.Websites;

namespace GenHTTP.Api.Content
{

    public record PageAdditions
    (

        List<StyleReference> Styles,

        List<ScriptReference> Scripts

    )
    {

        public static PageAdditions Create() => new(new(), new());

    }

}
