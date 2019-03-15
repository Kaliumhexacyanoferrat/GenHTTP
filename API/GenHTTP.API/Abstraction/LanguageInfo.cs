using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using GenHTTP.Api.Configuration;

namespace GenHTTP.Api.Abstraction
{

    /// <summary>
    /// Allows you to specify language information following ISO 639-1/RFC 1766.
    /// </summary>
    public class LanguageInfo
    {
        private static Dictionary<string, string> _Languages;
        private static Dictionary<string, string> _Countries;
        private Language _Language;
        private Country _Country;
        private string _Subcode;
        private string _Code;

        #region constructors

        /// <summary>
        /// Create a new language info object following ISO 639-1.
        /// </summary>
        /// <param name="language">The language to store</param>
        /// <example>
        /// Examples for this type of language code are:
        /// 
        /// de
        /// en
        /// fr
        /// </example>
        public LanguageInfo(Language language)
        {
            _Language = language;
            _Country = Country.Unspecified;
        }

        /// <summary>
        /// Create a new language info object.
        /// </summary>
        /// <param name="code">The code of the language</param>
        /// <remarks>
        /// Use this method, if you want to need to convert a
        /// language string (e.g. de-CH) into a LanguageInfo object.
        /// </remarks>
        public LanguageInfo(string code)
        {
            _Code = code;
            _Country = Country.Unspecified;
        }

        /// <summary>
        /// Create a new language info object following RFC 1766.
        /// </summary>
        /// <param name="language">The language to store</param>
        /// <param name="country">The country to store</param>
        /// <example>
        /// Examples for this type of language code are:
        /// 
        /// de-DE
        /// de-CH
        /// en-US
        /// en-UK
        /// </example>
        public LanguageInfo(Language language, Country country) : this(language)
        {
            _Country = country;
        }

        /// <summary>
        /// Create a new language info object following RFC 1766.
        /// </summary>
        /// <param name="language">The language to store</param>
        /// <param name="subcode">The subcode to store (see example)</param>
        /// <remarks>
        /// If you need another subcode than a country code, you can use
        /// this constructor.
        /// </remarks>
        /// <example>
        /// This language combinations are valid after RFC 1766:
        /// 
        /// en-cockney
        /// zh-guoyu
        /// zh-min-nan
        /// zh-xiang
        /// </example>
        public LanguageInfo(Language language, string subcode) : this(language)
        {
            _Subcode = subcode;
        }

        /// <summary>
        /// Create a new language info object following RFC 1766.
        /// </summary>
        /// <param name="code">The language code</param>
        /// <param name="subcode">The subcode to store</param>
        /// <remarks>
        /// If you want to create a language info object not following the
        /// ISO 639-1 standard, you can use this constructor.
        /// </remarks>
        /// <example>
        /// This constructor allows you to create language combinations like these:
        /// 
        /// x-klingon
        /// i-sami-no
        /// </example>
        public LanguageInfo(string code, string subcode)
        {
            _Code = code;
            _Subcode = subcode;
        }

        #endregion

        #region get-/setters

        /// <summary>
        /// The language this object stores.
        /// </summary>
        public Language Language
        {
            get { return _Language; }
            set { _Language = value; }
        }

        /// <summary>
        /// The country this object stores.
        /// </summary>
        public Country Country
        {
            get { return _Country; }
            set { _Country = value; }
        }

        /// <summary>
        /// Set or get the language code.
        /// </summary>
        /// <remarks>
        /// Use 'x' for experimental and 'i' for IANA registered
        /// languages. The <see cref="Code" /> property will always prioritize
        /// over the <see cref="Language" /> property.
        /// </remarks>
        public string Code
        {
            get { return _Code; }
            set { _Code = value; }
        }

        /// <summary>
        /// Set or get the additional language code.
        /// </summary>
        /// <remarks>
        /// You can set this value to the name of a dialect or
        /// a experimental language (e.g. klingon). Note, that
        /// the <see cref="Subcode" /> property will always priorizize over
        /// the <see cref="Country" /> property.
        /// </remarks>
        public string Subcode
        {
            get { return _Subcode; }
            set { _Subcode = value; }
        }

        /// <summary>
        /// Convert the language and country information into a code
        /// following ISO 639-1/RFC 1766.
        /// </summary>
        public string LanguageString
        {
            get { return ToString(); }
        }

        #endregion

        /// <summary>
        /// Convert the language and country information into a code
        /// following ISO 639-1/RFC 1766.
        /// </summary>
        /// <returns>The coded string</returns>
        public override string ToString()
        {
            return Get(this);
        }

        #region static methods

        /// <summary>
        /// Retrieve a language string.
        /// </summary>
        /// <param name="languageInfo">The language info</param>
        /// <returns>The language string for this info object</returns>
        public static string Get(LanguageInfo languageInfo)
        {
            if (_Languages == null || _Countries == null) InitTables();
            if (languageInfo.Language == Language.Unspecified) throw new Exception("Given language is 'unspecified'");
            string ret = (languageInfo.Code != null) ? languageInfo.Code : _Languages[languageInfo.Language.ToString()];
            if (languageInfo.Subcode != null)
            {
                ret += "-" + languageInfo.Subcode;
            }
            else
            {
                if (languageInfo.Country != Country.Unspecified) ret += "-" + _Countries[languageInfo.Country.ToString()];
            }
            return ret;
        }

        private static void InitTables()
        {
            if (_Languages != null && _Countries != null) return;
            // load configuration files
            string path = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName.Replace("\\", "/") + "/";
            Setting languages = Setting.FromXml(path + "config/languages.xml");
            Setting countries = Setting.FromXml(path + "config/countries.xml");
            // init tables
            _Languages = new Dictionary<string, string>(languages.Children.Count);
            _Countries = new Dictionary<string, string>(countries.Children.Count);
            // fill tables
            foreach (Setting language in languages.Children)
            {
                string name = language.Attributes["name"];
                if (!_Languages.ContainsKey(name)) _Languages.Add(name, language.Attributes["code"]);
            }
            foreach (Setting country in countries.Children)
            {
                string name = country.Attributes["name"];
                if (!_Countries.ContainsKey(name)) _Countries.Add(name, country.Attributes["code"]);
            }
        }

        #endregion

    }

}
