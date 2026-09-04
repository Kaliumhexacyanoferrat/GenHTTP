using GenHTTP.Api.Content;

using GenHTTP.Modules.Controllers;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Playground.Samples;

public static class WebserviceSample
{

    public static IHandlerBuilder Create()
    {
        /*
         *
         * Shows how to declare and register a class that will be invoked
         * to handle incoming HTTP requests.
         *
         * See https://genhttp.org/documentation/content/frameworks/webservices/
         *
         */

        return Layout.Create()
                     .AddService<BookService>("books");
    }

    public record Book(int Id, string Title);

    public class BookService
    {
        private static readonly List<Book> Books =
        [
            new(1, "Lord of the Rings")
        ];

        // GET http://localhost:8080/books/
        [ResourceMethod]
        public List<Book> List() => Books;

        // PUT http://localhost:8080/books/ with JSON/XML/... payload
        [ResourceMethod(Method.Put)]
        public Book Create(Book book)
        {
            var toAdd = book with
            {
                Id = Books.Max(b => b.Id) + 1
            };

            Books.Add(toAdd);
            return toAdd;
        }

        // DELETE http://localhost:8080/books/1
        [ResourceMethod(Method.Delete, ":id")]
        public void Delete([FromPath] int id)
        {
            Books.RemoveAll(b => b.Id == id);
        }

    }

}
