# GenHTTP.API

This package provides all public types that are required to develop new
modules for the GenHTTP application framework.

New modules should only rely on the API and other modules that they need
to provide their functionality.

New types may be added to the API if they are used by a large number of
different modules (think `IResource` or `IUser`).

The API is segmented into the following sections and namespaces:

| Namespace      | Description                                                                                                                    |
|----------------|--------------------------------------------------------------------------------------------------------------------------------|
| Protocol       | Defines the basic HTTP types, such as `IRequest` or `IResponse`.                                                               |
| Content        | Adds the `IHandler` / `IConcern` model and core concepts that are shared by multiple modules (such as `IResource` or `IUser`). |
| Routing        | Adds the [routing model](https://genhttp.org/documentation/content/concepts/routing/) and its basic implementation.            |
| Infrastructure | Defines the basic properties of a server, independent from the engine implementing it.                                         |
