using System.Globalization;

using GenHTTP.Api.Content;

using GenHTTP.Modules.Functional;
using GenHTTP.Modules.I18n;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Playground.Samples;

public static class LocalizationSample
{

    public static IHandlerBuilder Create()
    {
        /*
         *
         * Greets users in their preferred language and formats dates the way
         * they are used to. The language is taken from the "lang" query parameter
         * or the Accept-Language header sent by the browser.
         *
         * See https://genhttp.org/documentation/content/concerns/localization/
         *
         */

        var greetings = new Dictionary<string, string>
        {
            ["en"] = "Hello",
            ["de"] = "Hallo",
            ["fr"] = "Bonjour"
        };

        var greeting = Inline.Create()
                             .Get(() =>
                             {
                                 var culture = CultureInfo.CurrentCulture;

                                 return $"{greetings[culture.TwoLetterISOLanguageName]}! {DateTime.Now.ToString("D", culture)}";
                             });

        var localization = Localization.Create()
                                       .FromQuery("lang")
                                       .FromHeader()
                                       .Supports(culture => greetings.ContainsKey(culture.TwoLetterISOLanguageName)) // accepts regional variants such as de-CH
                                       .Default(CultureInfo.GetCultureInfo("en"))
                                       .Setter(currentCulture: true, currentUiCulture: true);

        return Layout.Create()
                     .Add("greeting", greeting) // GET http://localhost:8080/localization/greeting/?lang=fr
                     .Add(localization);
    }

}
