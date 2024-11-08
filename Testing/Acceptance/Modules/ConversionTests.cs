using System.Net;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Conversion.Serializers;
using GenHTTP.Modules.Conversion.Serializers.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules;

[TestClass]
public sealed class ConversionTests
{

    #region Helpers

    private static async Task RunTest<TFormat, TData>(string serialized) where TFormat : ISerializationFormat, new()
    {
        var handler = new ConversionHandlerBuilder<TData>(new TFormat());

        await using var runner = await TestHost.RunAsync(handler);

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

        public int field;
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
        private readonly ISerializationFormat _Format;

        public ConversionHandlerBuilder(ISerializationFormat format)
        {
            _Format = format;
        }

        public IHandler Build() => new ConversionHandler<T>(_Format);
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
    public async Task TestFormFieldSerialization() => await RunTest<FormFormat, FieldData>("field=20");

    [TestMethod]
    public async Task TestFormPropertySerialization() => await RunTest<FormFormat, PropertyData>("Field=20");

    [TestMethod]
    public async Task TestFormTypeSerialization() => await RunTest<FormFormat, TypedData>("Boolean=1&Double=0.2&String=Test&Enum=One");

    [TestMethod]
    public async Task TestFormDefaultValueSerialization() => await RunTest<FormFormat, TypedData>("Boolean=0&Double=0&String=&Enum=One");

    #endregion

}
