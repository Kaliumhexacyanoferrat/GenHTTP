using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

using GenHTTP.Utilities;

namespace GenHTTP.Abstraction
{

    /// <summary>
    /// Defines the encoding of a web page.
    /// </summary>
    public class DocumentEncoding
    {
        private static Dictionary<string, string> _Symbols;
        private DocumentEncodingType _Type;

        #region constructors

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <remarks>
        /// The default encoding is UTF-8.
        /// </remarks>
        public DocumentEncoding()
        {
            _Type = DocumentEncodingType.utf_8;
        }

        #endregion

        #region get-/setters

        /// <summary>
        /// Get or set the selected encoding.
        /// </summary>
        public DocumentEncodingType Type
        {
            get { return _Type; }
            set { _Type = value; }
        }

        /// <summary>
        /// The selected .NET encoding.
        /// </summary>
        public Encoding Encoding
        {
            get
            {
                return Encoding.GetEncoding(Name);
            }
        }

        /// <summary>
        /// Retrieve the name of the selected encoding.
        /// </summary>
        public string Name
        {
            get
            {
                return _Type.ToString().Replace("_", "-");
            }
        }

        #endregion

        #region symbol converter

        /// <summary>
        /// Convert special chars.
        /// </summary>
        /// <param name="symbol">The character to transform</param>
        /// <returns>The HTML entity string</returns>
        public static string ConvertSymbol(char symbol)
        {
            string str = symbol.ToString();
            if (_Symbols == null) InitSymbolTable();
            if (_Symbols.ContainsKey(str)) return _Symbols[str];
            return str;
        }

        /// <summary>
        /// Convert all special chars within a string.
        /// </summary>
        /// <param name="toConvert">The string to convert</param>
        /// <returns>The escaped string</returns>
        public static string ConvertString(string toConvert)
        {
            if (toConvert == null) throw new ArgumentException("toConvert");
            if (toConvert == "") return toConvert;
            if (_Symbols == null) InitSymbolTable();
            foreach (string symbol in _Symbols.Keys)
            {
                if (toConvert.Contains(symbol)) toConvert = toConvert.Replace(symbol, "&" + _Symbols[symbol] + ";");
            }
            return toConvert;
        }

        private static void InitSymbolTable()
        {
            if (_Symbols != null) return;
            // load html entities configuration file
            string path = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName.Replace("\\", "/") + "/";
            Setting entities = Setting.FromXml(path + "config/html-entities.xml");
            // fill symbol table
            _Symbols = new Dictionary<string, string>(entities.Children.Count);
            foreach (Setting entity in entities.Children)
            {
                if (!_Symbols.ContainsKey(entity.Attributes["symbol"])) _Symbols.Add(entity.Attributes["symbol"], entity.Attributes["name"]);
            }
        }

        #endregion

    }

}
