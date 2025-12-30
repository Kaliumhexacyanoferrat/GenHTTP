using GenHTTP.Api.Content;

namespace GenHTTP.Modules.Reflection;

/// <summary>
/// Basic protocol for all reflection based frameworks.
/// </summary>
/// <typeparam name="T">The actual builder class (e.g. "InlineBuilder")</typeparam>
public interface IReflectionFrameworkBuilder<out T>
    : IHandlerBuilder<T>, IRegistryBuilder<T>, IExecutionSettingsBuilder<T>
    where T : IHandlerBuilder<T>;
