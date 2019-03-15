using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ExampleProject.View;
using ExampleProject.Controller;

using GenHTTP;
using GenHTTP.Api.Compilation;
using GenHTTP.Api.Http;
using GenHTTP.Api.Content;

namespace ExampleProject
{

    /// <summary>
    /// This class describes a web application for the
    /// GenHTTP server.
    /// </summary>
    public class Project : ProjectBase
    {

        #region get-/setters

        /// <summary>
        /// The name of the project. Should not contain
        /// special characters or whitespace. The name of your
        /// project defines the URL on which your project
        /// can be accessed:
        /// 
        /// http://.../Name/
        /// </summary>
        public override string Name
        {
            get { return "ExampleProject"; }
        }

        /// <summary>
        /// The version of your project.
        /// </summary>
        public override string Version
        {
            get { return "2.0"; }
        }

        #endregion

        #region Templating

        /// <summary>
        /// Allows you to add templates to the project which
        /// can be used by content providers.
        /// </summary>
        protected override void InitTemplateBases()
        {
            // we will use this template later
            TemplateBases.Add("main", new ExampleTemplateBase());
        }

        /// <summary>
        /// Allows your content providers to retrieve a template
        /// to fill.
        /// </summary>
        /// <param name="name">the name of the template to retrieve</param>
        /// <returns>The requested template</returns>
        public override ITemplate GetTemplateInstance(string name)
        {
            if (name == "main") return new ExampleTemplate(TemplateBases["main"]);
            return null;
        }

        #endregion

        #region Snippets

        /// <summary>
        /// Allows you to load snippet bases. These are very
        /// similar to templates, but they do not represent
        /// a whole document.
        /// </summary>
        protected override void InitSnippetBases()
        {
            // Snippets.Add("mysnippet", new MySnippetBase());
        }

        /// <summary>
        /// Allows your content providers to recieve a new snippet.
        /// You need to add a line for every snippet your project
        /// providers.
        /// </summary>
        /// <param name="name">The name of the snippet to retrive</param>
        /// <returns>The requested snippet</returns>
        public override ISnippet GetSnippetInstance(string name)
        {
            // if (name == "mysnippet") return new MySnippet(Snippets[name]);
            return null;
        }

        #endregion

        #region Session manager

        /// <summary>
        /// This method should be used to initialize the session
        /// manager of the project which allows you to manage users,
        /// groups and sessions via a XML file.
        /// </summary>
        protected override void InitSessionManager()
        {
            // Init session manager
            // _Sessions = new SessionManager("sessions.xml");
        }

        #endregion

        #region Localization

        /// <summary>
        /// You can initialize the built-in localization manager
        /// here.
        /// </summary>
        protected override void InitLocalizationManager()
        {
            /*

            Therefor, you can set the _Localization variable of the project.

            */
            // Load localization from a file
            // _Localization = new LocalizationManager("myLocalization.xml");
        }

        #endregion

        #region Content providers

        /// <summary>
        /// This method allows you to add content providers to the project.
        /// Those are the controllers of our MVC pattern.
        /// 
        /// Every functionallity your project provides will be added
        /// here via a content provider.
        /// </summary>
        protected override void InitContentProviders()
        {
            // the folder provider allows you to provide a whole
            // folder to your clients
            _Providers.Add(new FolderProvider(this, "design", LocalFolder + "/design/"));
            // add your own providers here
            _Providers.Add(new IndexProvider(this));
        }

        #endregion

        #region Additional initialization

        /// <summary>
        /// This method will be called by the server to allow you to initialize
        /// some stuff which is not related to the other init methods.
        /// </summary>
        public override void InitOther()
        {

            /*

            If you uncomment this code, the project will compile a template from a
            generated document. That's how you can create templates.

            */

            /*
            Document doc = new Document();

            doc.Header.Title = new Placeholder(typeof(string), "Title") + " - ExampleProject";
            doc.Header.AddStylesheet("./design/style.css");

            doc.Body.AddHeadline(new Placeholder(typeof(string), "Headline").ToString());
            doc.Body.AddText(new Placeholder(typeof(NeutralElement), "Value").ToString());

            DocumentCompiler compiler = new DocumentCompiler(doc);
            compiler.Compile("ExampleTemplate.cs", "ExampleProject.View", "ExampleTemplate");
            */

        }

        /// <summary>
        /// Allows you to prepare a HTTP response before the request is handled.
        /// </summary>
        /// <param name="request">The request to analyze</param>
        /// <param name="response">The response to prepare</param>
        public override void PreHandleRequest(IHttpRequest request, IHttpResponse response)
        {
            // for example, we could set a header field for all CS files
            // if (request.File.EndsWith(".cs")) response.Header["Cache-Control"] = "no-cache";
        }

        #endregion

        #region Shutdown

        /// <summary>
        /// This method will be called if the server shuts down.
        /// </summary>
        public override void Dump()
        {
            this.Log.WriteLine("Shutdown!");
        }

        #endregion

    }

}
