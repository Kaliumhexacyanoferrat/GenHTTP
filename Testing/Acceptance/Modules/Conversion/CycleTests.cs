using GenHTTP.Modules.Conversion.Serializers;
using GenHTTP.Modules.Conversion.Serializers.Forms;
using GenHTTP.Modules.Conversion.Serializers.Json;
using GenHTTP.Modules.Conversion.Serializers.Xml;
using GenHTTP.Modules.Conversion.Serializers.Yaml;
using GenHTTP.Modules.Protobuf.Providers;
using ProtoBuf;

namespace GenHTTP.Testing.Acceptance.Modules.Conversion;

[TestClass]
public class CycleTests
{

    [ProtoContract]
    public class TestType
    {

        [ProtoMember(1)]
        public string? Message { get; set; }

    }

    [TestMethod]
    public async ValueTask TestFormats()
    {
        var obj = new TestType()
        {
            Message = "There and back"
        };

        var subjects = new ISerializationFormat[]
        {
            new JsonFormat(), new FormFormat(), new YamlFormat(), new XmlFormat(), new ProtobufFormat()
        };

        foreach (var subject in subjects)
        {
            var serialized = await subject.SerializeAsync(obj);

            using var stream = new MemoryStream(serialized.ToArray());

            var clone = (await subject.DeserializeAsync(stream, typeof(TestType)));

            Assert.AreEqual(obj.Message, ((TestType)clone!).Message);
        }
    }

}
