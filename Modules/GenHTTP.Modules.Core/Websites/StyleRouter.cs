using System.Collections.Generic;
using System.Linq;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Websites;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.Websites
{

    public class StyleRouter : IHandler
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        private Dictionary<string, Style> Styles { get; }

        public bool Empty => Styles.Count == 0;

        private IHandler Bundle { get; }

        #endregion

        #region Initialization

        public StyleRouter(IHandler parent, List<Style> styles)
        {
            Parent = parent;

            Styles = styles.ToDictionary(s => s.Name);

            var bundle = Core.Bundle.Create().ContentType(ContentType.TextCss);

            foreach (var style in Styles.Values)
            {
                bundle.Add(style.Provider);
            }

            Bundle = bundle.Build(this);
        }

        #endregion

        #region Functionality

        public List<StyleReference> GetReferences(bool bundle)
        {
            if (bundle)
            {
                return new List<StyleReference>
                {
                    new StyleReference("styles/bundle.css")
                };
            }

            return Styles.Values.Select(s => new StyleReference($"styles/{s.Name}")).ToList();
        }

        public IResponse? Handle(IRequest request)
        {
            /*
            current.Scope(this);

            if (!current.Request.Server.Development)
            {
                if (current.ScopedPath.EndsWith("bundle.css"))
                {
                    current.RegisterContent(Bundle);
                }
            }
            else if (Styles.TryGetValue(current.ScopedPath.Substring(1), out Style style))
            {
                current.RegisterContent(Download.From(style.Provider)
                                                .Type(ContentType.TextCss)
                                                .Build());
            }*/

            return null; // ToDo
        }

        public IEnumerable<ContentElement> GetContent(IRequest request)
        {
            // ToDo: Basepath
            return Styles.Values.Select(s => new ContentElement($"{s.Name}", s.Name, ContentType.TextCss, null));
        }

        #endregion

    }

}
