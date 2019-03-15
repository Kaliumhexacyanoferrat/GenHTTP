using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP;
using GenHTTP.Abstraction;
using GenHTTP.Abstraction.Elements;
using GenHTTP.Abstraction.Style;
using GenHTTP.Abstraction.Compiling;

namespace Utility
{

    public class ServerPageCompiler
    {

        public static void Compile()
        {
            try
            {
                Document doc = new Document();
                doc.Header.Favicon = "/favicon.ico";
                doc.Header.Title = "GenHTTP Webserver";
                doc.Body.BackgroundColor = new ElementColor(200, 200, 200);
                doc.Body.Color = new ElementColor(0, 0, 0);
                doc.Body.Font = new ElementFont("Tahoma", 10);

                doc.Body.AddNewLine(doc.Type, 2);

                Div title = doc.Body.AddDiv();
                title.Margin = new ElementPosition();
                title.Margin.Left = new ElementSize(10, ElementSizeType.Percent);
                title.Margin.Right = new ElementSize(10, ElementSizeType.Percent);
                title.Padding = new ElementPosition(5);
                title.BackgroundColor = new ElementColor(39, 151, 23);
                title.Color = ElementColor.White;
                title.Border = new ElementBorderCollection(1);
                title.Border.Value.Color = new ElementColor(39, 151, 23);
                title.FontWeight = ElementFontWeight.Bold;
                title.AddText(new Placeholder(typeof(string), "Title").ToString());

                Div content = doc.Body.AddDiv();
                content.Margin = new ElementPosition();
                content.Margin.Left = new ElementSize(10, ElementSizeType.Percent);
                content.Margin.Right = new ElementSize(10, ElementSizeType.Percent);
                content.Padding = new ElementPosition(5);
                content.Border = new ElementBorderCollection(1);
                content.Border.Value.Color = new ElementColor(39, 151, 23);
                content.BackgroundColor = ElementColor.White;
                content.AddText(new Placeholder(typeof(string), "Value").ToString());


                Div info = doc.Body.AddDiv();
                info.Margin = new ElementPosition();
                info.Margin.Left = new ElementSize(10, ElementSizeType.Percent);
                info.Margin.Right = new ElementSize(10, ElementSizeType.Percent);
                info.Padding = new ElementPosition(5);
                info.BackgroundColor = new ElementColor(39, 151, 23);
                info.Color = ElementColor.White;
                info.Border = new ElementBorderCollection(1);
                info.Border.Value.Color = new ElementColor(39, 151, 23);
                info.Font = new ElementFont(8);
                info.TextAlign = ElementTextAlign.Right;
                info.AddText("GenHTTP Webserver v" + new Placeholder(typeof(string), "ServerVersion"));

                doc.Body.AddNewLine(doc.Type, 2);

                Div valid = doc.Body.AddDiv();
                valid.Margin = new ElementPosition();
                valid.Margin.Left = new ElementSize(10, ElementSizeType.Percent);
                valid.Margin.Right = new ElementSize(10, ElementSizeType.Percent);
                valid.Padding = new ElementPosition(5);
                valid.TextAlign = ElementTextAlign.Center;
                valid.AddImage("/valid_xhtml.png", "Valid XHTML");
                valid.AddImage("/valid_css.gif", "Valid CSS");

                DocumentCompiler compiler = new DocumentCompiler(doc);

                if (!compiler.Compile(@"C:\Temp\ServerPage.cs", "GenHTTP", "ServerPage"))
                {
                    Console.WriteLine(compiler.Error.Message);
                    Console.WriteLine(compiler.Error.StackTrace);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

    }

}
