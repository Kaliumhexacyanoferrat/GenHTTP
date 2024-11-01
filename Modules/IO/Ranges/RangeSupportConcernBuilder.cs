using GenHTTP.Api.Content;

namespace GenHTTP.Modules.IO.Ranges;

public class RangeSupportConcernBuilder : IConcernBuilder
{

    #region Functionality

    public IConcern Build(IHandler content) => new RangeSupportConcern(content);

    #endregion

}
