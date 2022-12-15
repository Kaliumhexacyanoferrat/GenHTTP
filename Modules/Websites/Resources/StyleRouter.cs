using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Websites;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.Websites.Resources
{

    public sealed class StyleRouter : IHandler
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

            var bundle = Websites.Bundle.Create().ContentType(ContentType.TextCss, "UTF-8");

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

        public ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var file = request.Target.Current?.Value;

            if (file is not null)
            {
                if (!request.Server.Development)
                {
                    if (file == "bundle.css")
                    {
                        return Bundle.HandleAsync(request);
                    }
                }
                else if (Styles.TryGetValue(file, out Style? style))
                {
                    return Content.From(style.Provider)
                                   .Build(this)
                                   .HandleAsync(request);
                }
            }

            return new ValueTask<IResponse?>();
        }

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request)
        {
            var path = this.GetRoot(request, false);

            return Styles.Values.Select(s =>
            {
                var childPath = path.Edit(false)
                                    .Append(s.Name)
                                    .Build();

                var info = ContentInfo.Create()
                                      .Title(s.Name)
                                      .Build();

                return new ContentElement(childPath, info, ContentType.TextCss, null);
            }).ToAsyncEnumerable();
        }

        public ValueTask PrepareAsync() => Bundle.PrepareAsync();

        #endregion

    }

}
