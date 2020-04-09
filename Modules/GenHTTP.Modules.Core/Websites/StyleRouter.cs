using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Websites;
using GenHTTP.Api.Protocol;
using System.Collections.Generic;
using System.Linq;

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
            var file = request.Target.Current;

            if (file != null)
            {
                if (!request.Server.Development)
                {
                    if (file == "bundle.css")
                    {
                        return Bundle.Handle(request);
                    }
                }
                else if (Styles.TryGetValue(file, out Style style))
                {
                    return Download.From(style.Provider)
                                   .Type(ContentType.TextCss)
                                   .Build(this)
                                   .Handle(request);
                }
            }

            return null;
        }

        public IEnumerable<ContentElement> GetContent(IRequest request)
        {
            // ToDo: Basepath
            return Styles.Values.Select(s => new ContentElement($"{s.Name}", s.Name, ContentType.TextCss, null));
        }

        #endregion

    }

}
