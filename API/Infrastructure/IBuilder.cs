namespace GenHTTP.Api.Infrastructure;

/// <summary>
/// General interface implemented by every builder resposible to
/// configure and setup an instance.
/// </summary>
/// <typeparam name="T">The type which should be produced by the builder</typeparam>
/// <remarks>
/// Builders must not change their internal state when building an object, allowing
/// builder instances to be re-used if required.
/// </remarks>
public interface IBuilder<out T>
{

    /// <summary>
    /// Creates a new instance of the specified type using
    /// the current configuration.
    /// </summary>
    /// <returns>The newly created instance</returns>
    T Build();
}
