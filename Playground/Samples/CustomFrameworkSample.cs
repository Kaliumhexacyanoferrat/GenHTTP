using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.ApiBrowsing;
using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Functional.Provider;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.OpenApi;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Playground.Samples;

public static class CustomFrameworkSample
{

    public static IHandlerBuilder Create()
    {
        /*
         *
         * Shows how to implement a custom framework.
         *
         * See https://genhttp.org/documentation/content/frameworks/custom/
         *
         */

        return Layout.Create()
                     .Add(new CustomFrameworkHandler())
                     .AddOpenApi()
                     .AddScalar();
    }

    public class CustomFrameworkHandler : IHandler, IServiceMethodProvider
    {
        private MethodCollection? _methods;

        public MethodCollection Methods => _methods ?? throw new InvalidOperationException("Handler is not prepared yet");

        public async ValueTask PrepareAsync(IServer server)
        {
            var list = new List<MethodHandler>();

            // specify the supported methods of the operation we are exposing
            var supportedMethods = new MethodConfiguration([RequestMethod.Get]);

            // the actual piece of code to be executed - either a method info or a delegate
            var methodInfo = GetType().GetMethod("ExposedMethod")!;

            // auto enables code generation, otherwise reflection only
            var executionSettings = new ExecutionSettings(ExecutionMode.Auto);

            // configures the behavior for serialization, injection and formatting
            var registry = new MethodRegistry(
                Serialization.Default().Build(),
                Injection.Default().Build(),
                Formatting.Default().Build()
            );

            // build the operation we would like to provide
            var operation = OperationBuilder.Create(server, ":id", methodInfo, null, executionSettings, supportedMethods, registry);

            // create a method handler from the operation which will serve it as an HTTP endpoint
            // this handler requires an instance provider which tells the framework on which
            // object to invoke the given method info or delegate
            list.Add(new MethodHandler(operation, (_) => new(this), registry));

            // build a method collection handler from all collected operations
            // this handler is responsible for routing
            _methods = new MethodCollection(list);

            await _methods.PrepareAsync(server);
        }

        public ValueTask<IResponse?> HandleAsync(IRequest request) => Methods.HandleAsync(request);

        public string ExposedMethod(int id) => id.ToString();

    }

}
