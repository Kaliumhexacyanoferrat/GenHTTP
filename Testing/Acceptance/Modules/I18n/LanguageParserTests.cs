using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.I18n;
using GenHTTP.Modules.I18n.Parsers;
using GenHTTP.Modules.I18n.Provider;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;

namespace GenHTTP.Testing.Acceptance.Modules.I18n;

[TestClass]
public sealed class LanguageParserTests
{
    #region Helper method

    private static CultureInfo Culture(string name) => CultureInfo.CreateSpecificCulture(name);

    #endregion

    public static IEnumerable<(string? Input, CultureInfo[] ExpectedResult)> TestData =>
    [
        //Empty input
        new (null, []),
        new (string.Empty, []),
        new (" ", []),
        new (",", []),
        new (" ,", []),
        new (", ", []),
        new (" , ", []),

        //Invalid input
        new ("unknown", []),
        new ("unknown,zw", []),

        //Invalid quality
        new ("en;q=0_8,de;q=0#9", [Culture("en"), Culture("de")]),

        //Trailing/leading whitespaces
        new (" en", [Culture("en")]),
        new ("en ", [Culture("en")]),

        //Single language
        new ("en", [Culture("en")]),
        new ("eN", [Culture("en")]),
        new ("EN", [Culture("en")]),
        new ("en-US", [Culture("en-US")]),
        new ("en-UK", [Culture("en-UK")]),
        new ("en-uk", [Culture("en-uk")]),

        //Multiple languages
        new ("en,de", [Culture("en"), Culture("de")]),
        new ("en,de,", [Culture("en"), Culture("de")]),
        new (",en,de", [Culture("en"), Culture("de")]),
        new ("de,en", [Culture("de"), Culture("en")]),
        new ("en, de", [Culture("en"), Culture("de")]),
        new ("en, de, fr", [Culture("en"), Culture("de"), Culture("fr")]),
        new ("en,en,de", [Culture("en"), Culture("en"), Culture("de")]),

        new ("de,en-uk,fr", [Culture("de"), Culture("en-UK"), Culture("fr")]),
        new ("de, en-uk,fr", [Culture("de"), Culture("en-UK"), Culture("fr")]),
        new ("de,en-uk ,fr", [Culture("de"), Culture("en-UK"), Culture("fr")]),

        //Preference order
        new ("en;q=0.8,de;q=0.9", [Culture("de"), Culture("en")]),
        new ("en;q=0.9,de;q=0.8", [Culture("en"), Culture("de")]),
        new ("en;q=0.9,de;q=0.9", [Culture("en"), Culture("de")]),
        new ("en;q=0.9,de;q=0.9,fr;q=0.8", [Culture("en"), Culture("de"), Culture("fr")]),

        //Preference order mixed
        new ("en;q=0.9,fr;q=0.8,de", [Culture("de"), Culture("en"), Culture("fr")]),
        new ("en ;q=0.9,fr;q=0.8,de", [Culture("de"), Culture("en"), Culture("fr")]),
        new ("en; q=0.9 ,  fr; q=  0.8,  de", [Culture("de"), Culture("en"), Culture("fr")]),
        new ("en-UK;q=0.9,fr;q=0.8,de", [Culture("de"), Culture("en-uk"), Culture("fr")]),
        new ("en-UK;q=0.9,ww;q=0.8,de", [Culture("de"), Culture("en-uk")]),

        //Malformed
        new ("en;q=0.9,fr;;q=0.8,de", [Culture("fr"), Culture("de"), Culture("en")]),
        new ("en;q=0.9,frq=0.8,de", [Culture("de"), Culture("en")]),
    ];

    [TestMethod]
    [DynamicData(nameof(TestData))]
    public void TestLanguageParser(string input, CultureInfo[] expectedResult)
    {
        var actualResult = CultureInfoParser.ParseFromLanguage(input);

        CollectionAssert.AreEqual(expectedResult, actualResult);
    }
}
