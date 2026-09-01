using GenHTTP.Api.Content;

using GenHTTP.Modules.Controllers;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Reflection;

namespace GenHTTP.Playground.Samples;

public static class ControllerSample
{

    public static IHandlerBuilder Create()
    {
        /*
         *
         * Shows how to declare and register a controller that will be invoked
         * to handle incoming HTTP requests.
         *
         * See https://genhttp.org/documentation/content/frameworks/controllers/
         *
         */

        return Layout.Create()
                     .AddController<BookController>("books");
    }

    public record Book(int Id, string Title);

    public class BookController
    {
        private static readonly List<Book> Books =
        [
            new(1, "Lord of the Rings")
        ];

        // GET http://localhost:8080/books/
        [ControllerAction]
        public List<Book> Index() => Books;

        // PUT http://localhost:8080/books/create with JSON/XML/... payload
        [ControllerAction(Method.Put)]
        public Book Create(Book book)
        {
            var toAdd = book with
            {
                Id = Books.Max(b => b.Id) + 1
            };

            Books.Add(toAdd);
            return toAdd;
        }

        // DELETE http://localhost:8080/books/delete/1
        [ControllerAction(Method.Delete)]
        public void Delete([FromPath] int id)
        {
            Books.RemoveAll(b => b.Id == id);
        }

    }

}
