using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Conversion.Providers;
using GenHTTP.Modules.Conversion.Providers.Forms;
using System.Threading.Tasks;

namespace GenHTTP.Testing.Acceptance.Modules.Conversion
{

    [TestClass]
    public sealed class ConversionTests
    {

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
            private ISerializationFormat _Format;

            public ConversionHandlerBuilder(ISerializationFormat format)
            {
                _Format = format;
            }

            public IHandler Build(IHandler parent)
            {
                return new ConversionHandler<T>(_Format, parent);
            }

        }

        private class ConversionHandler<T> : IHandler
        {

            public IHandler Parent { get; }

            public ISerializationFormat Format { get; }
            
            public ValueTask PrepareAsync() => ValueTask.CompletedTask;

            public ConversionHandler(ISerializationFormat format, IHandler parent) 
            {
                Parent = parent;
                Format = format;
            }

            public IEnumerable<ContentElement> GetContent(IRequest request) => Enumerable.Empty<ContentElement>();

            public async ValueTask<IResponse?> HandleAsync(IRequest request)
            {
                var obj = await Format.DeserializeAsync(request.Content!, typeof(T));

                return (await Format.SerializeAsync(request, obj!)).Build();
            }

        }

        #endregion

        #region Tests

        [TestMethod]
        public void TestFormFieldSerialization() => RunTest<FormFormat, FieldData>("field=20");

        [TestMethod]
        public void TestFormPropertySerialization() => RunTest<FormFormat, PropertyData>("Field=20");

        [TestMethod]
        public void TestFormTypeSerialization() => RunTest<FormFormat, TypedData>("Boolean=1&Double=0.2&String=Test&Enum=One");

        #endregion

        #region Helpers

        private void RunTest<TFormat, TData>(string serialized) where TFormat : ISerializationFormat, new()
        {
            var handler = new ConversionHandlerBuilder<TData>(new TFormat());

            using var runner = TestRunner.Run(handler);

            var request = runner.GetRequest();

            request.Method = "POST";

            using (var stream = request.GetRequestStream())
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(serialized);
                }
            }

            using var response = request.GetSafeResponse();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(serialized, response.GetContent());
        }

        #endregion

    }

}
