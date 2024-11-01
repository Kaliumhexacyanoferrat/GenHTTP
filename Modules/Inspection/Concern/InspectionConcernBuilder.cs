using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Conversion.Serializers;
using GenHTTP.Modules.Conversion.Serializers.Yaml;

namespace GenHTTP.Modules.Inspection.Concern;

public sealed class InspectionConcernBuilder : IConcernBuilder
{
    private SerializationRegistry _Serialization;

    #region Initialization

    public InspectionConcernBuilder()
    {
        _Serialization = Serialization.Empty()
                                      .Default(ContentType.ApplicationYaml)
                                      .Add(ContentType.ApplicationYaml, new YamlFormat())
                                      .Build();
    }

    #endregion

    #region Functionality

    public IConcern Build(IHandler content)
    {
        return new InspectionConcern(content, _Serialization);
    }

    #endregion

}
