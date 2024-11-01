using GenHTTP.Api.Content;

namespace GenHTTP.Modules.Security.Providers;

public class SnifferPreventionConcernBuilder : IConcernBuilder
{

    #region Functionality

    public IConcern Build(IHandler content) => new SnifferPreventionConcern(content);

    #endregion

}
