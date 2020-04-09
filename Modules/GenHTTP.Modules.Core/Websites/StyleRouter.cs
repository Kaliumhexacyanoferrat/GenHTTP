﻿using System.Collections.Generic;
using System.Linq;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Content.Websites;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.Core.General;

namespace GenHTTP.Modules.Core.Websites
{

    public class StyleRouter : RouterBase
    {

        #region Get-/Setters

        private Dictionary<string, Style> Styles { get; }

        public bool Empty => Styles.Count == 0;

        private IContentProvider Bundle { get; }

        #endregion

        #region Initialization

        public StyleRouter(List<Style> styles,
                           IRenderer<TemplateModel>? template,
                           IContentProvider? errorHandler) : base(template, errorHandler)
        {
            Styles = styles.ToDictionary(s => s.Name);

            var bundle = Core.Bundle.Create().ContentType(ContentType.TextCss);

            foreach (var style in Styles.Values)
            {
                bundle.Add(style.Provider);
            }

            Bundle = bundle.Build();
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

        public override void HandleContext(IEditableRoutingContext current)
        {
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
            }
        }

        public override IEnumerable<ContentElement> GetContent(IRequest request, string basePath)
        {
            return Styles.Values.Select(s => new ContentElement($"{basePath}{s.Name}", s.Name, ContentType.TextCss, null));
        }

        public override string? Route(string path, int currentDepth)
        {
            return Parent.Route(path, currentDepth);
        }

        #endregion

    }

}
