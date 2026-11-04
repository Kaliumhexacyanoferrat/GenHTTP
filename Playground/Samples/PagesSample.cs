using Cottle;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Functional;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Pages;

namespace GenHTTP.Playground.Samples;

public static class PagesSample
{

    public static IHandlerBuilder Create()
    {
        /*
         *
         * Renders simple HTML pages on the server, e.g. for an internal admin
         * tool that does not justify a frontend framework.
         *
         * Templates are usually embedded into the assembly and loaded via
         * Resource.FromAssembly("Books.html").
         *
         * See https://genhttp.org/documentation/content/handlers/pages/
         *
         */

        var template = Renderer.From(Resource.FromString(BooksTemplate).Build());

        var books = Inline.Create()
                          .Get(async (IRequest request) =>
                          {
                              var model = new Dictionary<Value, Value>
                              {
                                  ["books"] = Value.FromEnumerable(Books.Select(b => Value.FromDictionary(new Dictionary<Value, Value>
                                  {
                                      ["id"] = b.Id,
                                      ["title"] = b.Title.Escaped() // the template engine does not escape HTML for you
                                  })))
                              };

                              return request.GetPage(await template.RenderAsync(model));
                          });

        var about = Inline.Create()
                          .Get(async (IRequest request) => request.GetPage(await Renderer.Server.RenderAsync("About", "<p>This library runs on GenHTTP.</p>")));

        return Layout.Create()
                     .Add("books", books) // GET http://localhost:8080/pages/books/ (rendered from a Cottle template)
                     .Add("about", about); // GET http://localhost:8080/pages/about/ (in the style of the server's own pages)
    }

    public record Book(int Id, string Title);

    private static readonly List<Book> Books =
    [
        new(1, "Lord of the Rings"),
        new(2, "Pride & Prejudice")
    ];

    private const string BooksTemplate =
        """
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="utf-8">
            <title>Books</title>
        </head>
        <body>
            <table>
                <tr><th>ID</th><th>Title</th></tr>
                {for book in books:
                <tr><td>{book.id}</td><td>{book.title}</td></tr>
                }
            </table>
        </body>
        </html>
        """;

}
