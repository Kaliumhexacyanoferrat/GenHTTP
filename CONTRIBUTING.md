# Contributing

The GenHTTP project is happy about all contributions such as bug fixes, improvements or additional features.

## Getting Started

To add your contributions to the GenHTTP webserver, create a fork of this repository and add your changes there. As soon
as your work has finished, create a pull request to allow a maintainer to review and merge your changes into the `main`
branch.

If you need some inspiration for your first contribution, have a look at the issues labeled
as [good first issues](https://github.com/Kaliumhexacyanoferrat/GenHTTP/issues?q=is%3Aopen+is%3Aissue+label%3A%22good+first+issue%22).

For larger changes or new features, please open an issue first so that the approach can be discussed before
you invest time into the implementation.

## Definition of Done

For a pull request to be merged into `main`, the following general rules need to be fulfilled:

- The automated build was able to compile and successfully test the changes on all platforms
- New behavior and bug fixes are covered by acceptance tests that run against all server engines
- The test coverage on new code as reported by Sonar is not below 85%
- There are no new issues reported by Sonar
- Public API functions and types are documented
- `IHandler` and `IConcern` instances can be created using an `IBuilder` instance
- The public API stays compatible, or breaking changes have been agreed upon with a maintainer
- Changes to the request handling path do not introduce performance regressions
- Architecture principles as described below have not been violated
- The pull request describes changes visible to users (new API, changed behavior or defaults), so that the
  maintainers can update the [documentation](https://genhttp.org/documentation/) accordingly

## Quality Goals

GenHTTP is a library that other projects build on, so the following goals apply to every change. If they conflict,
they are prioritized in this order:

1. **Correctness and security**: The server follows the HTTP specifications and handles requests from untrusted
   clients safely. Defaults are secure; less secure behavior has to be enabled explicitly.
2. **Compatibility**: Public APIs follow semantic versioning. Applications run on all supported engines and
   platforms with the same behavior.
3. **Performance**: The request handling path is designed to be allocation-free. Changes to this path should be
   backed by benchmarks.
4. **Ease of use**: Functionality is exposed via fluent builders and factory methods with sensible defaults.

## Project Structure

The GenHTTP webserver is designed as a small core that can be extended by additional modules where required. The
following table describes the projects and their purpose.

| Project                       | Description                                                                                                                                                                                                       |
|-------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `GenHTTP.Api`                 | Public interface of the GenHTTP webserver. Contains types that are useful for all kind of content providers. Problem specific types and references to 3rd party libraries are not allowed here.                   |
| `GenHTTP.Engine.*`            | Server engines (`Internal`, `Kestrel`, `Ioxide`) and the functionality they share (`Shared`). Engines may only reference a small set of basic modules and must not depend on optional functionality (e.g. `GenHTTP.Modules.Websockets`). |
| `GenHTTP.Modules.*`           | Additional modules that can be used to add functionality to the webserver. References to 3rd party libraries are allowed. Must not reference `GenHTTP.Engine.*` to stay independent from the server implementation. May reference other modules as needed. |
| `GenHTTP.Adapters.*`          | Integrations that allow GenHTTP handlers to run within other frameworks such as ASP.NET Core.                                                                                                                     |
| `GenHTTP.Testing`             | Allows applications to test their GenHTTP handlers.                                                                                                                                                               |

The acceptance tests in `Testing/Acceptance` mirror this structure. [AGENTS.md](./AGENTS.md) describes the
architecture, conventions and tooling in more detail and is useful for human contributors as well.

## Pull Requests

- Keep a pull request focused on a single change
- Use a title in the form of `type(scope): Summary`, e.g. `fix(reflection): Interceptors can now be used with more
  than one parameter`, where `type` is one of `feat`, `fix`, `perf`, `refactor`, `test` or `chore`
- Please do not report security vulnerabilities via public issues or pull requests, but contact a maintainer
  privately

## Coding Agents

You are welcome to use coding agents to work on GenHTTP. [AGENTS.md](./AGENTS.md) provides them with the
necessary context and is picked up automatically by most tools. Please note that you remain responsible for
your contribution: review the generated code, make sure you understand it, and verify that it fulfills the
Definition of Done just like any hand-written change.

## Adding Modules

Modules can either be added to this repository or can be implemented in their own GitHub repository. As a general rule
of thumb, only modules of general purpose will be added to this repository (e.g. a widely used compression algorithm).
Modules maintained by this repository will automatically be released as nuget packages, whereas external modules need
their own release process. It is recommended to prefix packages with `GenHTTP.Modules` to increase their visibility. For
compatibility reasons, modules should use the lowest version of the GenHTTP API that allows the module to provide its
functionality.

## Legal Aspects

Contributing to GenHTTP does currently not require you to sign a CLA. Nevertheless, please
consider the following aspects when contributing to the project:

### Code of Conduct

To maintain the integrity of the project, please do not submit contributions:

- That you do not own or have rights to.
- That violate the intellectual property of others.
- That might lead to legal disputes or damage the reputation of the project.

By contributing, you certify that your submission does not violate any of the above and is free from legal entanglements.

### Licensing Implications

Before contributing, please ensure you understand the implications of submitting code under the 
MIT license. This means your code can be used, modified, and redistributed by anyone, including
for commercial purposes, as long as the MIT license terms are followed.

By submitting contributions, you agree to license them under the MIT license, as specified in
this repository. This means you cannot later revoke or modify the license for contributions
you've already made.

### Ownership of Contributions

By contributing to this project, you confirm that you are the sole owner of the code you submit 
or have received explicit permission from the rightful owner to contribute it. If your contributions 
are part of your employment, you must ensure your employer has approved your contribution under
the terms of this project's license.

### Patent Licensing

By submitting your contributions, you agree that you will not include any code that is subject to
any patents or patent claims that you own or control. You also agree not to contribute any 
code that might infringe on any existing patents.

### License Compatibility

By submitting code, you confirm that it is your original work and that it has not been copied,
in whole or in part, from any other project, codebase, or source. You must not submit any code 
that is derived from or based on external sources unless you have explicit permission to do so 
and can verify that the code is compatible with the project's MIT license.

### Future License Changes

By contributing to this project, you acknowledge that the project maintainers may, at their
discretion, relicense the project under terms compatible with the original MIT license. 
Your contributions will remain under the same licensing terms as the project.
