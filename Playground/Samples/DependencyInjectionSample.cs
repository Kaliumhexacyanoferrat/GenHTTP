using GenHTTP.Api.Content;

using GenHTTP.Modules.DependencyInjection;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Playground.Samples;

public static class DependencyInjectionSample
{

    public static IHandlerBuilder Create()
    {
        /*
         *
         * Lets the framework create services and pass their dependencies, so the
         * services do not need to know how to construct them (and can be tested
         * with fakes).
         *
         * Dependency injection needs to be enabled on the host, see Services() below
         * and Program.cs:
         *
         *   Host.Create()
         *       .AddDependencyInjection(DependencyInjectionSample.Services())
         *
         * See https://genhttp.org/documentation/content/concepts/dependency-injection/
         *
         */

        return Layout.Create()
                     .AddDependentService<BookService>("books"); // GET http://localhost:8080/dependency-injection/books/
    }

    public static IServiceProvider Services()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IBookRepository, InMemoryBookRepository>()
                .AddSingleton(TimeProvider.System);

        return services.BuildServiceProvider();
    }

    public record Book(int Id, string Title, DateTimeOffset? Added = null);

    public interface IBookRepository
    {

        List<Book> GetAll();

        Book Add(Book book);

    }

    public class InMemoryBookRepository : IBookRepository
    {
        private readonly List<Book> _books = [new(1, "Lord of the Rings")];

        private readonly Lock _sync = new();

        public List<Book> GetAll()
        {
            lock (_sync)
            {
                return [.._books];
            }
        }

        public Book Add(Book book)
        {
            lock (_sync)
            {
                var toAdd = book with { Id = _books.Select(b => b.Id).DefaultIfEmpty().Max() + 1 };

                _books.Add(toAdd);

                return toAdd;
            }
        }

    }

    // dependencies can be injected via the constructor ...
    public class BookService(IBookRepository repository)
    {

        [ResourceMethod]
        public List<Book> List() => repository.GetAll();

        // ... or as a method parameter
        [ResourceMethod(Method.Put)]
        public Book Add(Book book, TimeProvider time) => repository.Add(book with { Added = time.GetUtcNow() });

    }

}
