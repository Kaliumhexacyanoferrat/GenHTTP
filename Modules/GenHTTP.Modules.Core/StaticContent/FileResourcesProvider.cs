using System.IO;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.Core.General;

namespace GenHTTP.Modules.Core.StaticContent
{

    public class FileResourcesProvider : RouterBase
    {

        #region Get-/Setters

        public DirectoryInfo Directory { get; }

        #endregion

        #region Initialization

        public FileResourcesProvider(DirectoryInfo directory,
                                     IRenderer<TemplateModel>? template,
                                     IContentProvider? errorHandler) : base(template, errorHandler)
        {
            Directory = directory;
        }

        #endregion

        #region Functionality

        public override void HandleContext(IEditableRoutingContext current)
        {
            current.Scope(this);

            var file = Path.Combine(Directory.FullName, current.ScopedPath.Substring(1));

            if (File.Exists(file))
            {
                current.RegisterContent(Download.FromFile(file).Build());
            }
        }

        public override string? Route(string path, int currentDepth)
        {
            return Parent.Route(path, currentDepth);
        }

        #endregion

    }

}
