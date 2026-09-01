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
        private readonly List<Book> _books =
        [
            new(1, "Lord of the Rings")
        ];

        // GET http://localhost:8080/books/
        [ResourceMethod]
        public List<Book> List() => _books;

        // PUT http://localhost:8080/books/ with JSON/XML/... payload
        [ResourceMethod(Method.Put)]
        public Book Create(Book book)
        {
            var toAdd = book with
            {
                Id = _books.Max(b => b.Id) + 1
            };

            _books.Add(toAdd);
            return toAdd;
        }

        // DELETE http://localhost:8080/books/1
        [ResourceMethod(Method.Delete, ":id")]
        public void Delete([FromPath] int id)
        {
            _books.RemoveAll(b => b.Id == id);
        }

    }

}
