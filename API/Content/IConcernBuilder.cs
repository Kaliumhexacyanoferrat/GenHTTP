namespace GenHTTP.Api.Content;

/// <summary>
/// Interface which needs to be implementd by factories providing concern instances.
/// </summary>
public interface IConcernBuilder
{

    /// <summary>
    /// Builds the concern and sets the inner handler of it.
    /// </summary>
    /// <param name="parent">The parent of the resulting concern handler instance</param>
    /// <param name="contentFactory">A method providing the content of the concern</param>
    /// <returns>The newly created concern instance</returns>
    IConcern Build(IHandler parent, Func<IHandler, IHandler> contentFactory);
}
