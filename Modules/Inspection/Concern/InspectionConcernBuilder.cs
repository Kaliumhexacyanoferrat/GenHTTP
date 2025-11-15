using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Conversion.Serializers;
using GenHTTP.Modules.Conversion.Serializers.Yaml;
using Conv = GenHTTP.Modules.Conversion;

namespace GenHTTP.Modules.Inspection.Concern;

public sealed class InspectionConcernBuilder : IConcernBuilder
{
    private SerializationRegistry? _serialization;

    #region Functionality

    public InspectionConcernBuilder Serialization(SerializationRegistry registry)
    {
        _serialization = registry;
        return this;
    }

    public IConcern Build(IHandler content)
    {
        var serialization = _serialization ?? Conv.Serialization.Empty()
                                                  .Default(ContentType.ApplicationYaml)
                                                  .Add(ContentType.ApplicationYaml, new YamlFormat())
                                                  .Build();

        return new InspectionConcern(content, serialization);
    }

    #endregion

}
