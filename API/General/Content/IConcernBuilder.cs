namespace GenHTTP.Api.Content;

/// <summary>
/// Interface which needs to be implemented by factories providing concern instances.
/// </summary>
public interface IConcernBuilder
{

    IConcern Build(IHandler content);

}
