using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.ApiBrowsing;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.OpenApi;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Playground.Samples;

public static class ApiDocumentationSample
{

    public static IHandlerBuilder Create()
    {
        /*
         *
         * Generates an Open API definition for a web service and lets developers
         * explore and try the API in their browser. The viewers are hosted by the
         * server itself, so no internet connection is required.
         *
         * Works the same way for functional handlers and controllers.
         *
         * See https://genhttp.org/documentation/content/concerns/open-api/
         * and https://genhttp.org/documentation/content/handlers/api-browsing/
         *
         */

        return Layout.Create()
                     .AddService<BookService>("books")
                     .AddOpenApi() // GET http://localhost:8080/api-docs/openapi.json (or .yaml)
                     .AddSwaggerUi() // GET http://localhost:8080/api-docs/swagger/
                     .AddRedoc() // GET http://localhost:8080/api-docs/redoc/
                     .AddScalar(title: "Book API"); // GET http://localhost:8080/api-docs/scalar/
    }

    public record Book(int Id, string Title);

    public class BookService
    {
        private static readonly List<Book> Books =
        [
            new(1, "Lord of the Rings")
        ];

        [ResourceMethod]
        public List<Book> List() => Books;

        [ResourceMethod(":id")]
        public Book Get(int id)
            => Books.FirstOrDefault(b => b.Id == id) ?? throw new ProviderException(ResponseStatus.NotFound, $"There is no book with id {id}");

        [ResourceMethod(Method.Put)]
        public Book Create(Book book)
        {
            var toAdd = book with
            {
                Id = Books.Select(b => b.Id).DefaultIfEmpty().Max() + 1
            };

            Books.Add(toAdd);

            return toAdd;
        }

        [ResourceMethod(Method.Delete, ":id")]
        public void Delete(int id)
        {
            Books.RemoveAll(b => b.Id == id);
        }

    }

}
