# GenHTTP.Engine.Shared

Provides functionality that can be used by
a server engine implementation to reduce
boilerplate code.

Implementing an engine to be used with
the GenHTTP framework will usually involve
the following steps:

- Implement your `IServer` variant
- Derive your own `ServerHost` from the `ServerHost` base class provided by this library
- Add your main entry point by adding `Host.Create()`, returning your `ServerHost` implementation

Within your server implementation, you might want to
make use of the default interface implementations such
as `ResponseBuilder`, `Response` or the collections for
headers and queries.
