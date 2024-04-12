using System.Collections.Generic;

using GenHTTP.Api.Content.Websites;

namespace GenHTTP.Api.Content
{

    /// <summary>
    /// Additional information that needs to be applied to
    /// a page being rendered within a template.
    /// </summary>
    /// <param name="Styles">Additional style references to be rendered</param>
    /// <param name="Scripts">Additional script references to be rendered</param>
    public record PageAdditions
    (

        List<StyleReference> Styles,

        List<ScriptReference> Scripts

    )
    {

        /// <summary>
        /// Creates an empty page addition object.
        /// </summary>
        /// <returns>The newly created object</returns>
        public static PageAdditions Create() => new(new(), new());

    }

}
