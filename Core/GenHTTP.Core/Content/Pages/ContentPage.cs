using System;
using System.IO;
using System.Reflection;
using System.Text;

using GenHTTP.Api.Content.Pages;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Core.Content.Pages
{

    public class ContentPage : IContentPage
    {
        private const string TEMPLATE_NAME = "GenHTTP.Core.Content.Pages.ContentPage.html";

        #region Get-/Setters

        public string Title { get; set; }

        public string Content { get; set; }

        public IServer Server { get; }

        #endregion

        #region Initialization

        public ContentPage(IServer server)
        {
            Server = server;

            Title = "";
            Content = "";
        }

        #endregion

        #region Functionality

        public Stream GetStream()
        {
            var utf8 = Encoding.UTF8;

            StringBuilder template;

            var temp = Assembly.GetExecutingAssembly().GetManifestResourceNames();

            using (var stream = Assembly.GetExecutingAssembly()
                                        .GetManifestResourceStream(TEMPLATE_NAME))
            {
                using (var reader = new StreamReader(stream, utf8))
                {
                    template = new StringBuilder(reader.ReadToEnd());
                }
            }

            template.Replace("%TITLE%", Title);
            template.Replace("%CONTENT%", Content);

            template.Replace("%VERSION%", Server.Version.ToString());

            return new MemoryStream(utf8.GetBytes(template.ToString()));
        }
        
        #endregion

    }

}
