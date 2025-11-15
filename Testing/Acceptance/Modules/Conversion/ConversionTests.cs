using System.Net;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Conversion.Serializers;
using GenHTTP.Modules.Conversion.Serializers.Forms;

namespace GenHTTP.Testing.Acceptance.Modules.Conversion;

[TestClass]
public sealed class ConversionTests
{

    #region Helpers

    private static async Task RunTest<TFormat, TData>(string serialized, TestEngine engine) where TFormat : ISerializationFormat, new()
    {
        var handler = new ConversionHandlerBuilder<TData>(new TFormat());

        await using var runner = await TestHost.RunAsync(handler, engine: engine);

        var request = runner.GetRequest();

        request.Method = HttpMethod.Post;
        request.Content = new StringContent(serialized);

        using var response = await runner.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);
        Assert.AreEqual(serialized, await response.GetContentAsync());
    }

    #endregion

    #region Supporting data structures

#pragma warning disable CS0649

    private class FieldData
    {

        public int Field;
    }

    private class PropertyData
    {

        public int Field { get; set; }
    }

    private class TypedData
    {

        public bool Boolean { get; set; }

        public double Double { get; set; }

        public string? String { get; set; }

        public EnumData Enum { get; set; }
    }

    private enum EnumData { One, Two }

#pragma warning restore CS0649

    private class ConversionHandlerBuilder<T> : IHandlerBuilder
    {
        private readonly ISerializationFormat _format;

        public ConversionHandlerBuilder(ISerializationFormat format)
        {
            _format = format;
        }

        public IHandler Build() => new ConversionHandler<T>(_format);
    }

    private class ConversionHandler<T> : IHandler
    {

        public ConversionHandler(ISerializationFormat format)
        {
            Format = format;
        }

        public ISerializationFormat Format { get; }

        public ValueTask PrepareAsync() => ValueTask.CompletedTask;

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var obj = await Format.DeserializeAsync(request.Content!, typeof(T));

            return (await Format.SerializeAsync(request, obj!)).Build();
        }
    }

    #endregion

    #region Tests

    [TestMethod]
    [MultiEngineTest]
    public async Task TestFormFieldSerialization(TestEngine engine) => await RunTest<FormFormat, FieldData>("field=20", engine);

    [TestMethod]
    [MultiEngineTest]
    public async Task TestFormPropertySerialization(TestEngine engine) => await RunTest<FormFormat, PropertyData>("Field=20", engine);

    [TestMethod]
    [MultiEngineTest]
    public async Task TestFormTypeSerialization(TestEngine engine) => await RunTest<FormFormat, TypedData>("Boolean=1&Double=0.2&String=Test&Enum=One", engine);

    [TestMethod]
    [MultiEngineTest]
    public async Task TestFormDefaultValueSerialization(TestEngine engine) => await RunTest<FormFormat, TypedData>("Boolean=0&Double=0&String=&Enum=One", engine);

    #endregion

}
