using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;

using GenHTTP.Api.Content.Documentation;

using GenHTTP.Modules.Mcp;

namespace GenHTTP.Testing.Acceptance.Modules.Mcp;

[TestClass]
public sealed class ValidationTests
{

    #region Supporting data structures

    public sealed record SkillLevel(string? Name, string? SkillId, [property: Range(1, 5)] int Level);

    public sealed record Ranked([Range(1, 3)] int Rank);

    public sealed record TreeNode(string Name, [property: MaxLength(2)] List<TreeNode>? Children);

    public sealed class SkillTools
    {

        [Tool]
        [Summary("Adds or changes skills")]
        public int UpsertSkills([Length(1, 3)] List<SkillLevel> skills) => skills.Count;

        [Tool]
        [Summary("Uses all kinds of constraints")]
        public string Constrained([Range(1, 100)] int limit,
                                  [StringLength(10, MinimumLength = 2)] string code,
                                  [RegularExpression("^[a-z]+$")] string slug,
                                  [EmailAddress] string mail,
                                  [Url] string? link,
                                  [AllowedValues("a", "b")] string choice,
                                  [Range(0.5, 1.5)] double factor,
                                  [Range(1, 10, MinimumIsExclusive = true, MaximumIsExclusive = true)] int exclusive,
                                  [MinLength(1)] string[] tags,
                                  [MaxLength(2)] string[] few,
                                  [Range(typeof(DateTime), "2020-01-01", "2030-01-01")] DateTime when,
                                  [Required] string? must = null) => "ok";

        [Tool]
        [Summary("Validates a parameter-level attribute of a record")]
        public int Rank(Ranked ranked) => ranked.Rank;

        [Tool]
        [Summary("Validates a recursive structure")]
        public string Tree(TreeNode root) => root.Name;

    }

    #endregion

    #region Helpers

    private static Task<TestHost> RunAsync() => TestHost.RunAsync(Tools.Create().Add<SkillTools>());

    private static async Task<JsonNode> GetSchemaAsync(TestHost host, string tool)
    {
        var response = await McpClient.Modern(host).RequestAsync("tools/list");

        return response.Result["tools"]!.AsArray().Single(t => t!["name"]!.GetValue<string>() == tool)!["inputSchema"]!;
    }

    private static JsonObject ValidArguments() => new()
    {
        ["limit"] = 10,
        ["code"] = "abc",
        ["slug"] = "slug",
        ["mail"] = "a@b.ch",
        ["choice"] = "a",
        ["factor"] = 1.0,
        ["exclusive"] = 5,
        ["tags"] = new JsonArray("x"),
        ["few"] = new JsonArray("x"),
        ["when"] = "2025-01-01T00:00:00",
        ["must"] = "set"
    };

    #endregion

    #region Schema

    [TestMethod]
    public async Task TestNestedConstraintsAndNullables()
    {
        await using var host = await RunAsync();

        var skills = (await GetSchemaAsync(host, "upsert_skills"))["properties"]!["skills"]!;

        Assert.AreEqual(1, skills["minItems"]!.GetValue<int>());
        Assert.AreEqual(3, skills["maxItems"]!.GetValue<int>());

        var item = skills["items"]!;

        Assert.AreEqual("""["level"]""", item["required"]!.ToJsonString());
        Assert.AreEqual(1, item["properties"]!["level"]!["minimum"]!.GetValue<int>());
        Assert.AreEqual(5, item["properties"]!["level"]!["maximum"]!.GetValue<int>());
    }

    [TestMethod]
    public async Task TestParameterConstraints()
    {
        await using var host = await RunAsync();

        var properties = (await GetSchemaAsync(host, "constrained"))["properties"]!;

        Assert.AreEqual(1, properties["limit"]!["minimum"]!.GetValue<int>());
        Assert.AreEqual(100, properties["limit"]!["maximum"]!.GetValue<int>());

        Assert.AreEqual(2, properties["code"]!["minLength"]!.GetValue<int>());
        Assert.AreEqual(10, properties["code"]!["maxLength"]!.GetValue<int>());

        Assert.AreEqual("^[a-z]+$", properties["slug"]!["pattern"]!.GetValue<string>());
        Assert.AreEqual("email", properties["mail"]!["format"]!.GetValue<string>());
        Assert.AreEqual("uri", properties["link"]!["format"]!.GetValue<string>());
        Assert.AreEqual("""["a","b"]""", properties["choice"]!["enum"]!.ToJsonString());

        Assert.AreEqual(0.5, properties["factor"]!["minimum"]!.GetValue<double>());
        Assert.AreEqual(1.5, properties["factor"]!["maximum"]!.GetValue<double>());

        Assert.AreEqual(1, properties["exclusive"]!["exclusiveMinimum"]!.GetValue<int>());
        Assert.AreEqual(10, properties["exclusive"]!["exclusiveMaximum"]!.GetValue<int>());

        Assert.AreEqual(1, properties["tags"]!["minItems"]!.GetValue<int>());
        Assert.AreEqual(2, properties["few"]!["maxItems"]!.GetValue<int>());

        Assert.IsNull(properties["when"]!["minimum"]);
    }

    [TestMethod]
    public async Task TestParameterLevelRecordConstraints()
    {
        await using var host = await RunAsync();

        var rank = (await GetSchemaAsync(host, "rank"))["properties"]!["ranked"]!["properties"]!["rank"]!;

        Assert.AreEqual(3, rank["maximum"]!.GetValue<int>());
    }

    #endregion

    #region Validation

    [TestMethod]
    public async Task TestOptionalNullables()
    {
        await using var host = await RunAsync();

        var response = await McpClient.Modern(host).CallAsync("upsert_skills", new JsonObject
        {
            ["skills"] = new JsonArray(new JsonObject { ["name"] = "Kotlin", ["level"] = 4 })
        });

        Assert.IsFalse(response.IsError, response.Text);
        Assert.AreEqual("1", response.Text);
    }

    [TestMethod]
    public async Task TestNonNullablesStayRequired()
    {
        await using var host = await RunAsync();

        var response = await McpClient.Modern(host).CallAsync("upsert_skills", new JsonObject
        {
            ["skills"] = new JsonArray(new JsonObject { ["name"] = "Kotlin" })
        });

        Assert.IsTrue(response.IsError);
        AssertX.Contains("level", response.Text);
    }

    [TestMethod]
    public async Task TestAllViolationsAreReported()
    {
        await using var host = await RunAsync();

        var response = await McpClient.Modern(host).CallAsync("upsert_skills", new JsonObject
        {
            ["skills"] = new JsonArray(new JsonObject { ["name"] = "A", ["level"] = 9 },
                                       new JsonObject { ["name"] = "B", ["level"] = 1 },
                                       new JsonObject { ["name"] = "C", ["level"] = 0 },
                                       new JsonObject { ["name"] = "D", ["level"] = 2 })
        });

        Assert.IsTrue(response.IsError);

        Assert.AreEqual("The arguments are invalid:\n" +
                        "- skills: The field skills must be a string or collection type with a minimum length of '1' and maximum length of '3'.\n" +
                        "- skills[0].level: The field level must be between 1 and 5.\n" +
                        "- skills[2].level: The field level must be between 1 and 5.", response.Text);
    }

    [TestMethod]
    public async Task TestValidArguments()
    {
        await using var host = await RunAsync();

        var response = await McpClient.Modern(host).CallAsync("constrained", ValidArguments());

        Assert.IsFalse(response.IsError, response.Text);
    }

    [TestMethod]
    [DataRow("limit", "0")]
    [DataRow("code", "\"a\"")]
    [DataRow("slug", "\"UPPER\"")]
    [DataRow("mail", "\"no-mail\"")]
    [DataRow("link", "\"no link\"")]
    [DataRow("choice", "\"c\"")]
    [DataRow("factor", "2.0")]
    [DataRow("exclusive", "1")]
    [DataRow("tags", "[]")]
    [DataRow("few", "[\"a\",\"b\",\"c\"]")]
    [DataRow("when", "\"2040-01-01T00:00:00\"")]
    [DataRow("must", "null")]
    public async Task TestViolations(string argument, string value)
    {
        await using var host = await RunAsync();

        var arguments = ValidArguments();

        arguments[argument] = JsonNode.Parse(value);

        var response = await McpClient.Modern(host).CallAsync("constrained", arguments);

        Assert.IsTrue(response.IsError, $"Expected '{argument}' = {value} to be rejected");
        AssertX.Contains($"- {argument}: ", response.Text);
    }

    [TestMethod]
    public async Task TestParameterLevelRecordValidation()
    {
        await using var host = await RunAsync();

        var response = await McpClient.Modern(host).CallAsync("rank", new JsonObject { ["ranked"] = new JsonObject { ["rank"] = 7 } });

        Assert.IsTrue(response.IsError);
        AssertX.Contains("ranked.rank", response.Text);
    }

    [TestMethod]
    public async Task TestRecursiveValidation()
    {
        await using var host = await RunAsync();

        var tooMany = new JsonArray(new JsonObject { ["name"] = "a" }, new JsonObject { ["name"] = "b" }, new JsonObject { ["name"] = "c" });

        var response = await McpClient.Modern(host).CallAsync("tree", new JsonObject
        {
            ["root"] = new JsonObject { ["name"] = "root", ["children"] = new JsonArray(new JsonObject { ["name"] = "child", ["children"] = tooMany }) }
        });

        Assert.IsTrue(response.IsError);
        AssertX.Contains("root.children[0].children", response.Text);

        var valid = await McpClient.Modern(host).CallAsync("tree", new JsonObject { ["root"] = new JsonObject { ["name"] = "root" } });

        Assert.AreEqual("root", valid.Text);
    }

    #endregion

}
