# Contributing

The GenHTTP project is happy about all contributions like bug fixes, improvements or additional features.

## Getting Started

To add your contributions to the GenHTTP webserver, create a fork of this repository and add your changes there. As soon as your work has finished, create a pull request to allow a maintainer to review and merge your changes into the master branch.

If you need some inspiration for your first contribution, have a look at the issues labeled with [good first issue](https://github.com/Kaliumhexacyanoferrat/GenHTTP/issues?q=is%3Aopen+is%3Aissue+label%3A%22good+first+issue%22).

## Definition of Done

For a pull request to be merged into master, the following general rules need to be fulfilled:

- The automated build was able to compile and successfully test the changes
- The test coverage on new code as reported by Sonar is not below 85%
- There are no new issues reported by Sonar
- Public API functions and types are documented
- Providers and handlers can be created using an `IBuilder` instance
- The changes have been documented on the [GenHTTP.Website](https://github.com/Kaliumhexacyanoferrat/GenHTTP.Website), if applicable
- Architecture principles as described in the following section have not been violated

## Project Structure

The GenHTTP webserver is designed as a single library that can be extended by additional modules where required. The following table describes the projects and their purpose.

Project | Description
--- | ---
`GenHTTP.Api` | Public interface of the GenHTTP webserver. Contains types that are useful for all kind of content providers. Specific types and references to 3rd party libraries are not allowed here.
`GenHTTP.Engine` | The actual implementation of the webserver. References to 3rd party libraries or complex modules (e.g. `GenHTTP.Modules.Websites`) are not allowed.
`GenHTTP.Modules.*` | Additional modules that can be used to add functionality to the webserver. References to 3rd party libraries are allowed. Must not reference `GenHTTP.Engine` to stay independent from the server implementation.

## Adding Modules

Modules can either be added to this repository or can be implemented in their own GitHub repository. As a general rule of thumb, only modules of general purpose will be added to this repository (e.g. a widely used compression algorithm). Modules maintained by this repository will automatically be released as nuget packages, whereas external modules need their own release process. It is recommended to prefix packages with `GenHTTP.Modules` to increase their visibility. For compatibility reasons, modules should use the lowest version of the GenHTTP API that allows the module to provide it's functionality.
