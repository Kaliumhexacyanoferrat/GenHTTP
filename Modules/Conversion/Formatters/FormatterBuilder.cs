using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Conversion.Formatters;

public sealed class FormatterBuilder : IBuilder<FormatterRegistry>
{
    private readonly List<IFormatter> _registry = [];

    #region Functionality

    /// <summary>
    /// Adds the given formatter to the registry.
    /// </summary>
    /// <param name="formatter">The formatter to be added</param>
    public FormatterBuilder Add(IFormatter formatter)
    {
        _registry.Add(formatter);
        return this;
    }

    public FormatterBuilder Add<T>() where T : IFormatter, new() => Add(new T());

    /// <summary>
    /// Builds the formatter registry based on the configuration.
    /// </summary>
    /// <returns>The newly created formatter registry</returns>
    public FormatterRegistry Build() => new(_registry);

    #endregion

}
