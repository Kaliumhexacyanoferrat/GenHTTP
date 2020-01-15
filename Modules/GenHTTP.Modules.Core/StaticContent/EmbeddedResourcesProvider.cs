﻿using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using GenHTTP.Api.Protocol;
using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.Core.General;

namespace GenHTTP.Modules.Core.StaticContent
{

    public class EmbeddedResourcesProvider : RouterBase
    {

        #region Get-/Setters

        private Dictionary<string, IContentProvider> QualifiedNames { get; }
        
        #endregion

        #region Initialization

        public EmbeddedResourcesProvider(Assembly assembly,
                                         string root,
                                         IRenderer<TemplateModel>? template,
                                         IContentProvider? errorHandler) : base(template, errorHandler)
        {
            QualifiedNames = assembly.GetManifestResourceNames()
                                     .Where(n => n.Contains(root))
                                     .Select(n => new
                                     {
                                         Key = n.Substring(n.IndexOf(root) + root.Length),
                                         Value = Download.FromResource(assembly, n).Build()
                                     })
                                     .ToDictionary(n => n.Key!, n => n.Value!);
        }

        #endregion

        #region Functionality

        public override void HandleContext(IEditableRoutingContext current)
        {
            current.Scope(this);

            var identifier = current.ScopedPath.Replace('/', '.');

            if (QualifiedNames.ContainsKey(identifier))
            {
                current.RegisterContent(QualifiedNames[identifier]);
            }
        }

        public override IEnumerable<ContentElement> GetContent(IRequest request, string basePath)
        {
            foreach (var qn in QualifiedNames)
            {
                var slashPath = qn.Key.Replace('.', '/');
                var fileName = Path.GetFileName(slashPath);

                yield return new ContentElement(basePath + slashPath, fileName, fileName.GuessContentType() ?? ContentType.ApplicationForceDownload, null);
            }
        }

        public override string? Route(string path, int currentDepth)
        {
            return Parent.Route(path, currentDepth);
        }
        
        #endregion

    }

}
