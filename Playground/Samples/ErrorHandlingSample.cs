using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.ErrorHandling;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Playground.Samples;

public static class ErrorHandlingSample
{

    public static IHandlerBuilder Create()
    {
        /*
         *
         * A shop with a website for humans and an API for machines. Errors in the API
         * are reported as JSON (or XML, ... as requested by the client), while the website
         * renders HTML error pages. Exceptions thrown by the business logic are mapped
         * to HTTP status codes without the business logic knowing about HTTP.
         *
         * See https://genhttp.org/documentation/content/concerns/error-handling/
         *
         */

        var orders = Inline.Create()
                           // GET http://localhost:8080/errors/api/orders/42 (404 as JSON)
                           .Get(":id", (int id) => (id == 1) ? new Order(1, "Coffee") : throw new ProviderException(ResponseStatus.NotFound, $"There is no order with id {id}"))
                           // POST http://localhost:8080/errors/api/orders/ (409 via the custom mapper)
                           .Post((Order order) => Shop.PlaceOrder(order))
                           .Add(ErrorHandler.From(new OutOfStockMapper()));

        var website = Layout.Create()
                            .Index(Content.From(Resource.FromString("<h1>Welcome to our shop!</h1>").Type(ContentType.TextHtml)))
                            .Add(ErrorHandler.Html());

        return Layout.Create()
                     .Add("api/orders", orders)
                     .Add(website); // e.g. GET http://localhost:8080/errors/does-not-exist (404 as HTML page)
    }

    public record Order(int Id, string Product);

    public class OutOfStockException(string product) : Exception($"Sorry, '{product}' is sold out")
    {

        public string Product => product;

    }

    public static class Shop
    {

        public static Order PlaceOrder(Order order) => throw new OutOfStockException(order.Product);

    }

    public class OutOfStockMapper : IErrorMapper<OutOfStockException>
    {

        public ValueTask<IResponse?> Map(IRequest request, IHandler handler, OutOfStockException error, ByteString? acceptedFormat)
        {
            var response = request.Respond()
                                  .Status(ResponseStatus.Conflict)
                                  .Content(error.Message, ContentType.TextPlain)
                                  .Build();

            return new(response);
        }

        // null passes the request on, so the default error handling renders the 404
        public ValueTask<IResponse?> GetNotFound(IRequest request, IHandler handler, ByteString? acceptedFormat) => new();

    }

}
