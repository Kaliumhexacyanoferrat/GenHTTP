using GenHTTP.Api.Content;

using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Playground.Samples;

public static class FunctionalSample
{

    public static IHandlerBuilder Create()
    {
        /*
         *
         * Shows how to use delegates that will be invoked
         * to handle incoming HTTP requests.
         *
         * See https://genhttp.org/documentation/content/frameworks/functional/
         *
         */

        var books = new List<Book>()
        {
            new(1, "Lord of the Rings")
        };

        var service = Inline.Create()
                            .Get(() => books) // GET http://localhost:8080/books/
                            .Put((Book book) => // PUT http://localhost:8080/books/ with JSON/XML/... payload
                            {
                                var toAdd = book with
                                {
                                    Id = books.Max(b => b.Id) + 1
                                };

                                books.Add(toAdd);
                                return toAdd;
                            })
                            .Delete(":id", (int id) => books.RemoveAll(b => b.Id == id)); // DELETE http://localhost:8080/books/1

        return Layout.Create()
                     .Add("books", service);
    }

    public record Book(int Id, string Title);

}
