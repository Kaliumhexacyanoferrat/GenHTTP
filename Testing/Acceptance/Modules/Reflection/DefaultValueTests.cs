using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Generation;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Testing.Acceptance.Modules.Reflection;

[TestClass]
public sealed class DefaultValueTests
{

    #region Supporting data structures

    public enum SortOrder
    {
        Ascending,
        Descending
    }

    public sealed class DefaultResource
    {

        [ResourceMethod("int")]
        public int GetInt(int limit = 50) => limit;

        [ResourceMethod("bool")]
        public bool GetBool(bool descending = true) => descending;

        [ResourceMethod("string")]
        public string GetString(string name = "fallback") => name;

        [ResourceMethod("enum")]
        public SortOrder GetEnum(SortOrder order = SortOrder.Descending) => order;

        [ResourceMethod("double")]
        public double GetDouble(double factor = 1.5) => factor;

        [ResourceMethod("decimal")]
        public decimal GetDecimal(decimal amount = 2.5m) => amount;

        [ResourceMethod("long")]
        public long GetLong(long big = 5_000_000_000L) => big;

        [ResourceMethod("nullable")]
        public string GetNullable(int? limit = 7) => limit?.ToString() ?? "null";

        [ResourceMethod("nan")]
        public double GetNaN(double value = double.NaN) => value;

        [ResourceMethod("date")]
        public string GetDate([Optional, DateTimeConstant(630822816000000000)] DateTime date) => date.ToString("yyyy-MM-dd");

        [ResourceMethod("short")]
        public short GetShort(short value = -3) => value;

        [ResourceMethod("nullable-enum")]
        public string GetNullableEnum(SortOrder? order = SortOrder.Descending) => order?.ToString() ?? "null";

        [ResourceMethod(Method.Post, "body")]
        public int PostBody([FromBody] int value = 42) => value;

        [ResourceMethod("mixed")]
        public string GetMixed(int offset, int limit = 50, bool descending = true) => $"{offset}|{limit}|{descending}";

    }

    #endregion

    #region Tests

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestIntDefault(ServerEngine engine, ExecutionMode mode)
    {
        await AssertResultAsync(engine, mode, "/t/int", "50");
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestIntDefaultOverridden(ServerEngine engine, ExecutionMode mode)
    {
        await AssertResultAsync(engine, mode, "/t/int?limit=10", "10");
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestIntDefaultWithEmptyValue(ServerEngine engine, ExecutionMode mode)
    {
        await AssertResultAsync(engine, mode, "/t/int?limit=", "50");
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestBoolDefault(ServerEngine engine, ExecutionMode mode)
    {
        await AssertResultAsync(engine, mode, "/t/bool", "1");
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestBoolDefaultOverridden(ServerEngine engine, ExecutionMode mode)
    {
        await AssertResultAsync(engine, mode, "/t/bool?descending=false", "0");
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestStringDefault(ServerEngine engine, ExecutionMode mode)
    {
        await AssertResultAsync(engine, mode, "/t/string", "fallback");
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestEnumDefault(ServerEngine engine, ExecutionMode mode)
    {
        await AssertResultAsync(engine, mode, "/t/enum", "Descending");
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestDoubleDefault(ServerEngine engine, ExecutionMode mode)
    {
        await AssertResultAsync(engine, mode, "/t/double", "1.5");
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestDecimalDefault(ServerEngine engine, ExecutionMode mode)
    {
        await AssertResultAsync(engine, mode, "/t/decimal", "2.5");
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestLongDefault(ServerEngine engine, ExecutionMode mode)
    {
        await AssertResultAsync(engine, mode, "/t/long", "5000000000");
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestNullableDefault(ServerEngine engine, ExecutionMode mode)
    {
        await AssertResultAsync(engine, mode, "/t/nullable", "7");
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestNaNDefault(ServerEngine engine, ExecutionMode mode)
    {
        await AssertResultAsync(engine, mode, "/t/nan", "NaN");
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestNonLiteralDefault(ServerEngine engine, ExecutionMode mode)
    {
        await AssertResultAsync(engine, mode, "/t/date", "2000-01-01");
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestShortDefault(ServerEngine engine, ExecutionMode mode)
    {
        await AssertResultAsync(engine, mode, "/t/short", "-3");
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestNullableEnumDefault(ServerEngine engine, ExecutionMode mode)
    {
        await AssertResultAsync(engine, mode, "/t/nullable-enum", "Descending");
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestBodyDefault(ServerEngine engine, ExecutionMode mode)
    {
        var app = Layout.Create()
                        .AddService<DefaultResource>("t", mode: mode);

        await using var runner = await TestHost.RunAsync(app, engine: engine);

        var request = runner.GetRequest("/t/body");

        request.Method = HttpMethod.Post;
        request.Content = new StringContent(string.Empty, null, "text/plain");

        using var response = await runner.GetResponseAsync(request);

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("42", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestMixedDefaults(ServerEngine engine, ExecutionMode mode)
    {
        await AssertResultAsync(engine, mode, "/t/mixed?offset=3", "3|50|True");
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestInlineDefault(ServerEngine engine, ExecutionMode mode)
    {
        var inline = Inline.Create()
                           .Get((int limit = 50) => limit)
                           .ExecutionMode(mode);

        await using var runner = await TestHost.RunAsync(inline, engine: engine);

        using var response = await runner.GetResponseAsync("/");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("50", await response.GetContentAsync());
    }

    [TestMethod]
    public void TestLiteralGeneration()
    {
        Assert.AreEqual("\"a\\\"b\"", CompilationUtil.GetLiteral("a\"b"));
        Assert.AreEqual("true", CompilationUtil.GetLiteral(true));
        Assert.AreEqual("false", CompilationUtil.GetLiteral(false));
        Assert.AreEqual("-5", CompilationUtil.GetLiteral(-5));
        Assert.AreEqual("5U", CompilationUtil.GetLiteral(5u));
        Assert.AreEqual("5L", CompilationUtil.GetLiteral(5L));
        Assert.AreEqual("5UL", CompilationUtil.GetLiteral(5UL));
        Assert.AreEqual("2.5M", CompilationUtil.GetLiteral(2.5m));
        Assert.AreEqual("1.5F", CompilationUtil.GetLiteral(1.5f));
        Assert.AreEqual("((byte)7)", CompilationUtil.GetLiteral((byte)7));
        Assert.AreEqual("((short)-3)", CompilationUtil.GetLiteral((short)-3));
        Assert.AreEqual("((GenHTTP.Testing.Acceptance.Modules.Reflection.DefaultValueTests.SortOrder)(1))", CompilationUtil.GetLiteral(SortOrder.Descending));

        Assert.AreEqual("'c'", CompilationUtil.GetLiteral('c'));
        Assert.AreEqual("double.NaN", CompilationUtil.GetLiteral(double.NaN));
        Assert.AreEqual("double.NegativeInfinity", CompilationUtil.GetLiteral(double.NegativeInfinity));
        Assert.AreEqual("float.PositiveInfinity", CompilationUtil.GetLiteral(float.PositiveInfinity));

        Assert.IsNull(CompilationUtil.GetLiteral(new DateTime(2000, 1, 1)));
    }

    private static async Task AssertResultAsync(ServerEngine engine, ExecutionMode mode, string path, string expected)
    {
        var app = Layout.Create()
                        .AddService<DefaultResource>("t", mode: mode);

        await using var runner = await TestHost.RunAsync(app, engine: engine);

        using var response = await runner.GetResponseAsync(path);

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual(expected, await response.GetContentAsync());
    }

    #endregion

}
