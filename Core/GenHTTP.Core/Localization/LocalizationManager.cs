using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

using GenHTTP.Utilities;

namespace GenHTTP.Localization {
  
  /// <summary>
  /// The LocalizationManager allows you to use different languages
  /// for the output of your web application.
  /// </summary>
  /// <remarks>
  /// If you want to use the LocalizationManager for your own web
  /// application, you need to create a new XML file or to embedd
  /// the localization section into another XML file. Example
  /// for a localization XML file:
  /// 
  /// <example>
  /// &lt;localization default="deutsch"&gt;
  ///   &lt;localization language="common"&gt;
  ///     &lt;blubb&gt;Blubb&lt;/blubb&gt;
  ///   &lt;/localization&gt;
  ///   &lt;localization language="deutsch"&gt;
  ///     &lt;welcome&gt;Willkommen&lt;/welcome&gt;
  ///   &lt;/localization&gt;
  ///   &lt;localization language="english"&gt;
  ///     &lt;welcome&gt;Welcome&lt;/welcome&gt;
  ///   &lt;/localization&gt;
  /// &lt;/localization&gt;
  /// </example>
  /// </remarks>
  public class LocalizationManager {
    private Setting _EntryPoint;
    private Dictionary<string, Localization> _Languages;
    private bool _FromFile = false;

    /// <summary>
    /// Create a new LocalizationManager object from a XML file.
    /// </summary>
    /// <param name="sourceFile">The source file to read from</param>
    public LocalizationManager(string sourceFile) {
      _FromFile = true;
      _EntryPoint = Setting.FromXml(sourceFile);
      if (_EntryPoint.Name != "localization") throw new InvalidLocalizationSourceException("The given XML file is no localization source");
      LoadLanguages();
    }

    /// <summary>
    /// Create a new LocalizationManager from a XML section.
    /// </summary>
    /// <param name="source">The source entry to read from</param>
    public LocalizationManager(Setting source) {
      _EntryPoint = source;
      if (_EntryPoint.Name != "localization") throw new InvalidLocalizationSourceException("The given configuration seems to be no valid localization source");
      LoadLanguages();
    }

    private void LoadLanguages() {
      _Languages = new Dictionary<string, Localization>(_EntryPoint.Children.Count);
      foreach (Setting localization in _EntryPoint.Children["localization", 0]) {
        if (_Languages.ContainsKey(localization.Attributes["language"])) throw new InvalidLocalizationSourceException("The given source contains more than one entry for the language '" + localization.Attributes["language"] + "'");
        _Languages.Add(localization.Attributes["language"].ToLower(), new Localization(localization));
      }
      if (!_Languages.ContainsKey(_EntryPoint.Attributes["default"])) {
        _EntryPoint.Attributes["default"] = _Languages.Keys.ElementAt(0);
      }
    }

    #region get-/setters

    /// <summary>
    /// The localization for the default language.
    /// </summary>
    /// <remarks>
    /// If you set this value to a localization of another
    /// language, you will need to save this change.
    /// </remarks>
    public Localization DefaultLocalization {
      get { return _Languages[_EntryPoint.Attributes["default"]]; }
      set {
        if (value == null) throw new ArgumentNullException();
        _EntryPoint.Attributes["default"] = value.Name;
      }
    }

    /// <summary>
    /// The common part of the XML file.
    /// </summary>
    public Localization Common {
      get {
        if (!_Languages.ContainsKey("common")) throw new LanguageNotFoundException("This localization source seems to contain no 'common' section");
        return _Languages["common"];
      }
    }

    /// <summary>
    /// Retrieve all available localizations.
    /// </summary>
    public Collection<Localization> AvailableLocalizations {
      get {
        List<Localization> tmpList = new List<Localization>(_Languages.Values.Count);
        foreach (Localization loc in _Languages.Values) tmpList.Add(loc);
        return new Collection<Localization>(tmpList);
      }
    }

    /// <summary>
    /// Retrieve a localization by the name of its language.
    /// </summary>
    /// <param name="language">The name of the language</param>
    /// <returns>The requested localization object</returns>
    public Localization this[string language] {
      get {
        if (language == null || language == "") throw new ArgumentException();
        language = language.ToLower();
        if (!_Languages.ContainsKey(language)) throw new LanguageNotFoundException("The language '" + language + "' does not exist in this context");
        return _Languages[language];
      }
    }

    #endregion

    /// <summary>
    /// Try to find the requested localization of a <see cref="HttpRequest" />.
    /// </summary>
    /// <param name="request">The request to analyze</param>
    /// <returns>The localization for this request or the default localization</returns>
    /// <remarks>
    /// This method parses the URL of the <see cref="HttpRequest" /> to determine
    /// the name of the requested language. The URL has to be of this form: "/Project/language/*".
    /// If the requested file matches the regular expression, the method will redirect the request
    /// to "/Project/*" and will return the selected localization. Otherwise, the method
    /// will return the default localization.
    /// </remarks>
    public Localization LocalizeRequest(HttpRequest request) {
      Regex re = new Regex(@"^/([a-zA-Z0-9]+)/([a-zA-Z0-9]+)/");
      if (re.IsMatch(request.File)) {
        Match m = re.Match(request.File);
        Localization localization = GetLocalizationOrDefault(m.Groups[2].Value);
        request.Redirect("/" + m.Groups[1].Value + "/" + request.File.Substring(m.Length));
        return localization;
      }
      return DefaultLocalization;
    }

    /// <summary>
    /// Try to find the requested localization of a <see cref="HttpRequest" />.
    /// </summary>
    /// <param name="request">The request to analyze</param>
    /// <returns>The localization for this request or the default localization</returns>
    /// <remarks>
    /// This method will search for a GET field named "language" to determine the requested
    /// localization language. If the field is not set, the default localization will be returned.
    /// </remarks>
    public Localization LocalizeRequestViaGet(HttpRequest request) {
      if (request.GetFields.ContainsKey("language")) {
        return GetLocalizationOrDefault(request.GetFields["language"]);
      }
      return DefaultLocalization;
    }

    /// <summary>
    /// Retrieve the localization for a language or the default
    /// localization, if the language is not supported by this manager.
    /// </summary>
    /// <param name="language">The language to search for</param>
    /// <returns>The localization for the given language or the default localization</returns>
    public Localization GetLocalizationOrDefault(string language) {
      try {
        return this[language];
      }
      catch {
        return DefaultLocalization;
      }
    }

    /// <summary>
    /// Save the XML file to disk.
    /// </summary>
    /// <remarks>
    /// You should only call this method if you initialized the <see cref="LocalizationManager" /> with
    /// a XML file. Otherwise, an exception will be thrown.
    /// </remarks>
    public void Save() {
      if (!_FromFile) throw new NoXmlFileException("The localization source was not loaded from a XML file");
      _EntryPoint.ToXml();
    }

  }

  /// <summary>
  /// This exception will be thrown if the LocaliazationManager could
  /// not load the given ressource.
  /// </summary>
  public class InvalidLocalizationSourceException : Exception {

    /// <summary>
    /// Create a new instance of this exception.
    /// </summary>
    /// <param name="message">The message to display</param>
    public InvalidLocalizationSourceException(string message) : base(message) {}

  }

  /// <summary>
  /// This exception will be thrown if you try
  /// to save a <see cref="LocalizationManager" /> to disk
  /// without loaded it from a XML file.
  /// </summary>
  public class NoXmlFileException : Exception {

    /// <summary>
    /// Create a new instance of this exception.
    /// </summary>
    /// <param name="message">The message to display</param>
    public NoXmlFileException(string message) : base(message) { }

  }
  
  /// <summary>
  /// This exception will be thrown if you try to access a non existing
  /// localization.
  /// </summary>
  public class LanguageNotFoundException : Exception {

    /// <summary>
    /// Create a new instance of this exception.
    /// </summary>
    /// <param name="message">The message to display</param>
    public LanguageNotFoundException(string message) : base(message) { }

  }

}
