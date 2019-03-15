using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Globalization;

namespace GenHTTP.Api.Configuration
{

    /// <summary>
    /// Allows you to store settings or other data.
    /// </summary>
    public class Setting
    {
        /// <summary>
        /// The value of the setting.
        /// </summary>
        protected string _Value;
        /// <summary>
        /// The name of the setting entry.
        /// </summary>
        protected string _Name;
        /// <summary>
        /// The children of this setting.
        /// </summary>
        protected ChildCollection _Children;
        /// <summary>
        /// The attributes of this setting.
        /// </summary>
        protected AttributeCollection _Attributes;

        /// <summary>
        /// Create a new setting object.
        /// </summary>
        /// <param name="name">The name of the setting</param>
        public Setting(string name)
        {
            _Name = name;
            _Children = new ChildCollection();
            _Attributes = new AttributeCollection();
        }

        /// <summary>
        /// Create a new setting object with a pre-initialized value.
        /// </summary>
        /// <param name="name">The name of the setting</param>
        /// <param name="value">The initial value of the setting entry</param>
        public Setting(string name, string value) : this(name)
        {
            _Value = value;
        }

        #region get-/setters

        /// <summary>
        /// The value of this entry.
        /// </summary>
        public string Value
        {
            get { return _Value; }
            set { _Value = value; }
        }

        /// <summary>
        /// The name of this entry.
        /// </summary>
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        /// <summary>
        /// Retrieve all children of this setting element.
        /// </summary>
        public ChildCollection Children
        {
            get { return _Children; }
        }

        /// <summary>
        /// Retrieve the attributes of this element.
        /// </summary>
        public AttributeCollection Attributes
        {
            get { return _Attributes; }
        }

        /// <summary>
        /// Retrieve a child of this setting by its name.
        /// </summary>
        /// <param name="name">The name of the setting to find</param>
        /// <returns>The requested setting</returns>
        /// <remarks>
        /// If the setting entry does not exist yet, it will be created by the library.
        /// </remarks>
        public Setting this[string name]
        {
            get
            {
                return _Children[name];
            }
        }

        /// <summary>
        /// Retrieve a child of this setting by its name, and one of its attributes.
        /// </summary>
        /// <param name="name">The name of the setting to find</param>
        /// <param name="attribName">The name of the attribute to compare with</param>
        /// <param name="attribValue">The value of the attribute to compare with</param>
        /// <returns>The requested setting</returns>
        /// <remarks>
        /// If the setting entry does not exist yet, it will be created by the library.
        /// </remarks>
        public Setting this[string name, string attribName, string attribValue]
        {
            get
            {
                return _Children[name, attribName, attribValue];
            }
        }

        #endregion

        #region conversion support

        /// <summary>
        /// Try to convert the value of this setting into another type.
        /// </summary>
        /// <typeparam name="conversionType">The type to convert the value to</typeparam>
        /// <param name="defaultValue">If the conversion fails, this value will be returned</param>
        /// <returns>The converted value or defaultValue, if the conversion fails</returns>
        /// <remarks>
        /// Actually, you can convert the value of the setting entry only to basic types like
        /// int, double, float and so on. In some of the next versions, there will be an interface
        /// to convert complex types into settings and the other way arround (apart from IConvertible).
        /// 
        /// This method will use the culture invariant format provider to allow you to run your web application
        /// on different server machines using different cultures.
        /// </remarks>
        public conversionType ConvertTo<conversionType>(conversionType defaultValue)
        {
            try
            {
                return (conversionType)Convert.ChangeType(Value, typeof(conversionType), CultureInfo.InvariantCulture);
            }
            catch
            {
                return default(conversionType);
            }
        }

        /// <summary>
        /// Set the value of this setting.
        /// </summary>
        /// <typeparam name="conversionType">The type of the value to write</typeparam>
        /// <param name="value">The value to write</param>
        /// <remarks>
        /// This method will convert the given value into a string representation.
        /// </remarks>
        public void ConvertFrom<conversionType>(conversionType value)
        {
            _Value = Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Allows you to convert the value of this section into
        /// a DateTime object.
        /// </summary>
        /// <returns>The DateTime representation</returns>
        /// <remarks>
        /// If the setting is not in the right format, a call of
        /// this method will cause conversion exceptions.
        /// </remarks>
        public DateTime ToDateTime()
        {
            int day = Convert.ToInt32(Attributes["day"]);
            int month = Convert.ToInt32(Attributes["month"]);
            int year = Convert.ToInt32(Attributes["year"]);
            int hour = Convert.ToInt32(Attributes["hour"]);
            int minute = Convert.ToInt32(Attributes["minute"]);
            int second = Convert.ToInt32(Attributes["second"]);
            return new DateTime(year, month, day, hour, minute, second);
        }

        /// <summary>
        /// Set the settings value to a DateTime.
        /// </summary>
        /// <param name="value">The DateTime to set to</param>
        public void FromDateTime(DateTime value)
        {
            Attributes["day"] = value.Day.ToString();
            Attributes["month"] = value.Month.ToString();
            Attributes["year"] = value.Year.ToString();
            Attributes["hour"] = value.Hour.ToString();
            Attributes["minute"] = value.Minute.ToString();
            Attributes["second"] = value.Second.ToString();
        }

        #endregion

        #region XML support

        /// <summary>
        /// Load a XML file.
        /// </summary>
        /// <param name="file">The file to read from</param>
        /// <returns>A setting object representing the given XML file</returns>
        public static Setting FromXml(string file)
        {
            Setting retVal = null;
            if (File.Exists(file))
            {
                // use an parser to fill the setting from a XML file
                SettingParser parser = new SettingParser(file);
                retVal = parser.Fill();
            }
            else
            {
                // nothing to parse, create new
                retVal = new Setting("root");
            }
            retVal.Attributes["file"] = file;
            return retVal;
        }

        /// <summary>
        /// Save the setting object and all of its childs into a XML file.
        /// </summary>
        /// <param name="file">The file to write to</param>
        public void ToXml(string file)
        {
            if (file == null) throw new ArgumentException();
            XmlTextWriter w = new XmlTextWriter(file, Encoding.UTF8);
            w.WriteStartDocument();
            w.WriteStartElement(Name);
            foreach (string attrib in Attributes)
            {
                w.WriteStartAttribute(attrib);
                w.WriteString(Attributes[attrib]);
                w.WriteEndAttribute();
            }
            foreach (Setting s in Children) s.ToXml(w);
            w.WriteEndElement();
            w.WriteEndDocument();
            w.Close();
        }

        /// <summary>
        /// Save the settings to disk.
        /// </summary>
        /// <remarks>
        /// The setting needs to have the 'file' attribute set. If you load a XML file with the
        /// methods of this class, this attribute will be set for the root note.
        /// 
        /// <example>
        /// <code>
        /// // load a XML file
        /// Setting settings = Setting.FromXml("./file.xml");
        /// // add some settings
        /// Setting newSetting = new Setting("mySetting", "myValue");
        /// settings.Children.Add(newSetting);
        /// // save to disk
        /// settings.ToXml(); // works fine
        /// newSetting.ToXml(); // -> Exception, because the file attribute is not set!
        /// newSetting.ToXml("./file.xml"); // this will work
        /// </code>
        /// </example>
        /// </remarks>
        public void ToXml()
        {
            ToXml(Attributes["file"]);
        }

        private void ToXml(XmlTextWriter w)
        {
            w.WriteStartElement(Name);
            foreach (string attrib in Attributes)
            {
                w.WriteStartAttribute(attrib);
                w.WriteString(Attributes[attrib]);
                w.WriteEndAttribute();
            }
            foreach (Setting sub in Children)
            {
                sub.ToXml(w);
            }
            if (Value != null)
            {
                w.WriteValue(_Value);
            }
            w.WriteEndElement();
        }

        #endregion

    }

}
