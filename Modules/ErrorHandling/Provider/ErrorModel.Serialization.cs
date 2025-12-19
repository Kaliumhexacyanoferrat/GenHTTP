using System.Text.Json.Serialization;
using GenHTTP.Modules.ErrorHandling.Mappers;

namespace GenHTTP.Modules.ErrorHandling.Provider;

[JsonSerializable(typeof(StructuredErrorMapper.ErrorModel))]
public partial class ErrorHandlingContext : JsonSerializerContext
{

}
