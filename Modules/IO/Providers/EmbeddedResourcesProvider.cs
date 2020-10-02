using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.IO.Providers
{

    public class EmbeddedResourcesProvider : IHandler
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        private Dictionary<string, IHandler> QualifiedNames { get; }

        #endregion

        #region Initialization

        public EmbeddedResourcesProvider(IHandler parent, Assembly assembly, string root)
        {
            Parent = parent;

            QualifiedNames = assembly.GetManifestResourceNames()
                                     .Where(n => n.Contains(root))
                                     .Select(n => new
                                     {
                                         Key = n.Substring(n.IndexOf(root) + root.Length),
                                         Value = Download.FromResource(assembly, n).Build(this)
                                     })
                                     .ToDictionary(n => n.Key!, n => n.Value!);
        }

        #endregion

        #region Functionality

        public IResponse? Handle(IRequest request)
        {
            var identifier = request.Target.GetRemaining().ToString().Replace('/', '.');

            if (QualifiedNames.ContainsKey(identifier))
            {
                return QualifiedNames[identifier].Handle(request);
            }

            return null;
        }

        public IEnumerable<ContentElement> GetContent(IRequest request)
        {
            foreach (var qn in QualifiedNames)
            {
                var fileRef = ResourceToFile(qn.Key);

                var fileName = Path.GetFileName(fileRef);

                var path = new List<string>(this.GetRoot(request.Server.Handler, false).Parts);
                path.Add(fileRef);

                var info = ContentInfo.Create()
                                      .Title(fileName)
                                      .Build();

                yield return new ContentElement(new WebPath(path, false), info, fileName.GuessContentType() ?? ContentType.ApplicationForceDownload, null);
            }
        }

        private static string ResourceToFile(string resourceKey)
        {
            var replaced = resourceKey.Replace('.', '/');

            var editor = new StringBuilder(replaced);

            var index = replaced.LastIndexOf('/');

            if (index > 0)
            {
                editor[index] = '.';
            }

            return editor.ToString()
                         .Substring(1);
        }

        #endregion

    }

}
