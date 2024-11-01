using GenHTTP.Api.Content;
using GenHTTP.Modules.Inspection.Concern;

namespace GenHTTP.Modules.Inspection;

public static class Inspector
{

    #region Builder

    public static InspectionConcernBuilder Create() => new();

    #endregion

    #region Extensions

    public static T AddInspector<T>(this T builder) where T : IHandlerBuilder<T>
    {
        builder.Add(Create());
        return builder;
    }

    #endregion

}
