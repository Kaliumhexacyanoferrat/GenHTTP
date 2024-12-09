using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.I18n;
using GenHTTP.Modules.I18n.Provider;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;

namespace GenHTTP.Testing.Acceptance.Modules.I18n;

[TestClass]
public sealed class LocalizationTests
{
    [TestMethod]
    [MultiEngineTest]
    public async Task TestDefault(TestEngine engine)
    {
        var currentCulture = CultureInfo.CurrentUICulture;

        var localization = Localization.Create();

        await TestLocalization(engine, localization, _ =>
        {
            Assert.AreEqual(currentCulture, CultureInfo.CurrentUICulture);
        });
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestFromStatic(TestEngine engine)
    {
        var localization = Localization
            .Create()
            .FromRequest(_ => [CultureInfo.CreateSpecificCulture("fr")]);

        await TestLocalization(engine, localization, _ =>
        {
            Assert.AreEqual(CultureInfo.CreateSpecificCulture("fr"), CultureInfo.CurrentUICulture);
        });
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestFromQuery(TestEngine engine)
    {
        var localization = Localization
            .Create()
            .FromQuery();

        await TestLocalization(engine, localization,
        path: "?lang=cs-CZ",
        _ =>
        {
            Assert.AreEqual(CultureInfo.CreateSpecificCulture("cs-CZ"), CultureInfo.CurrentUICulture);
        });
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestFromHeader(TestEngine engine)
    {
        var localization = Localization
            .Create()
            .FromHeader();

        await TestLocalization(engine, localization,
        request =>
        {
            request.Headers.Add("Accept-Language", "mn-MN");
        },
        _ =>
        {
            Assert.AreEqual(CultureInfo.CreateSpecificCulture("mn-MN"), CultureInfo.CurrentUICulture);
        });
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestFromCookie(TestEngine engine)
    {
        var localization = Localization
            .Create()
            .FromCookie();

        await TestLocalization(engine, localization,
        request =>
        {
            request.Headers.Add("Cookie", "lang=quz-EC");
        },
        _ =>
        {
            Assert.AreEqual(CultureInfo.CreateSpecificCulture("quz-EC"), CultureInfo.CurrentUICulture);
        });
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSupportsList(TestEngine engine)
    {
        var localization = Localization
            .Create()
            .FromLanguage(_ => "en,de,cs,fr")
            .Supports([CultureInfo.CreateSpecificCulture("fr"), CultureInfo.CreateSpecificCulture("cs")]);

        await TestLocalization(engine, localization,
        _ =>
        {
            Assert.AreEqual(CultureInfo.CreateSpecificCulture("cs"), CultureInfo.CurrentUICulture);
        });
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSupportsPredicate(TestEngine engine)
    {
        var localization = Localization
            .Create()
            .FromLanguage(_ => "en,de,cs,fr")
            .Supports(culture => culture.Equals(CultureInfo.CreateSpecificCulture("fr")));

        await TestLocalization(engine, localization,
        _ =>
        {
            Assert.AreEqual(CultureInfo.CreateSpecificCulture("fr"), CultureInfo.CurrentUICulture);
        });
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSetterCurrentCulture(TestEngine engine)
    {
        var localization = Localization
            .Create()
            .FromRequest(_ => [CultureInfo.CreateSpecificCulture("fr")])
            .Setter(currentCulture: true, currentUICulture: false);

        await TestLocalization(engine, localization, _ =>
        {
            Assert.AreEqual(CultureInfo.CreateSpecificCulture("fr"), CultureInfo.CurrentCulture);
        });
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSetterCustom(TestEngine engine)
    {
        var localization = Localization
            .Create()
            .FromLanguage(_ => "de")
            .Setter((request, culture) => request.Properties["culture"] = culture);

        await TestLocalization(engine, localization, request =>
        {
            Assert.AreEqual(CultureInfo.CreateSpecificCulture("de"), request.Properties["culture"]);
        });
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestMultipleMixed(TestEngine engine)
    {
        var localization = Localization
            .Create()
            .FromQuery()            
            .FromCookie()
            .FromHeader()
            .FromLanguage(_ => "de")
            .Supports([CultureInfo.CreateSpecificCulture("de")])
            .Setter(currentCulture: true)
            .Setter((request, culture) => request.Properties["culture"] = culture);

        await TestLocalization(engine, localization,
        path: "?lang=cs-CZ",
        request =>
        {
            request.Headers.Add("Accept-Language", "mn-MN");
            request.Headers.Add("Cookie", "lang=quz-EC");
        },
        request =>
        {
            var expected = CultureInfo.CreateSpecificCulture("de");

            Assert.AreEqual(expected, request.Properties["culture"]);
            Assert.AreEqual(expected, CultureInfo.CurrentCulture);
            Assert.AreEqual(expected, CultureInfo.CurrentUICulture);
        });
    }

    private static Task TestLocalization(
        TestEngine engine,
        LocalizationConcernBuilder localization,
        Action<IRequest> requestAssert
        )
        => TestLocalization(engine, localization, null, null, requestAssert);

    private static Task TestLocalization(
        TestEngine engine,
        LocalizationConcernBuilder localization,
        string path,
        Action<IRequest> requestAssert
        )
        => TestLocalization(engine, localization, path, null, requestAssert);

    private static Task TestLocalization(
        TestEngine engine,
        LocalizationConcernBuilder localization,
        Action<HttpRequestMessage> requestSetup,
        Action<IRequest> requestAssert
        )
        => TestLocalization(engine, localization, null, requestSetup, requestAssert);

    private static async Task TestLocalization(
        TestEngine engine,
        LocalizationConcernBuilder localization,
        string? path,
        Action<HttpRequestMessage>? requestSetup,
        Action<IRequest> requestAssert
        )
    {
        Exception? assertException = null;

        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

        var handler = Inline
            .Create()
            .Add(localization)
            .Get((IRequest request) => 
            {
                try
                {
                    requestAssert(request);
                }
                catch (Exception e)
                {
                    assertException = e;
                }
            });

        await using var host = await TestHost.RunAsync(handler, engine: engine);

        using var request = host.GetRequest(path);
        requestSetup?.Invoke(request);

        using var _ = await host.GetResponseAsync(request);

        if (assertException != null)
        {
            throw assertException;
        }
    }
}
