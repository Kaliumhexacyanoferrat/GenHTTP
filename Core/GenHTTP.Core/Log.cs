using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Core
{

    /// <summary>
    /// Log information to a file.
    /// </summary>
    [Serializable]
    public class Log : ILog
    {
        private StreamWriter w;
        private bool _Console = true;
        private int _Written = 0;

        /// <summary>
        /// Create a new logfile.
        /// </summary>
        /// <param name="filename">The path of the log file to create</param>
        public Log(string filename)
        {
            try
            {
                w = new StreamWriter(filename, false);
                w.AutoFlush = true;
            }
            catch { }
        }

        /// <summary>
        /// Create a new logfile.
        /// </summary>
        /// <param name="filename">The path of the log file to create</param>
        /// <param name="log2console">Specifiy whether information should be logged to the console or not</param>
        public Log(string filename, bool log2console) : this(filename)
        {
            _Console = log2console;
        }

        /// <summary>
        /// Log some information.
        /// </summary>
        /// <param name="data">The text to log</param>
        public void WriteLine(string data)
        {
            if (_Console)
            {
                WriteTimestamp();
                Console.WriteLine(data);
            }
            if (w != null)
            {
                try
                {
                    w.WriteLine(DateTimeString + " " + data);
                }
                catch { }
            }
        }

        /// <summary>
        /// Write a colorized string to the console and the log file.
        /// </summary>
        /// <param name="data">The text to log</param>
        /// <param name="color">The color to use</param>
        public void WriteColored(string data, ConsoleColor color)
        {
            if (_Console)
            {
                Console.ForegroundColor = color;
                Console.Write(data);
                _Written += data.Length;
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            try
            {
                w.Write(data);
            }
            catch { }
        }

        /// <summary>
        /// Write a colorized line to the console and the log file.
        /// </summary>
        /// <param name="data">The text to log</param>
        /// <param name="color">The color to use</param>
        public void WriteLineColored(string data, ConsoleColor color)
        {
            if (_Console)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(data);
                Console.ForegroundColor = ConsoleColor.Gray;
                _Written = 0;
            }
            try
            {
                w.WriteLine(data);
            }
            catch { }
        }

        /// <summary>
        /// If you want to use the color based log functions, you will need to write the timestamp yourself. Use this method for this task.
        /// </summary>
        public void WriteTimestamp()
        {
            if (_Console)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(TimeString + " ");
                Console.ForegroundColor = ConsoleColor.Gray;
                _Written = TimeString.Length;
            }
            try
            {
                w.Write(DateTimeString + " ");
            }
            catch { }
        }

        /// <summary>
        /// Allows you to align a text right in the console.
        /// </summary>
        /// <param name="text">The text to write</param>
        /// <param name="color">The color in the console</param>
        public void WriteRightAlign(string text, ConsoleColor color)
        {
            if (_Console)
            {
                Console.ForegroundColor = color;
                int whitespaces = 80 - _Written - text.Length - 2;
                for (int i = 0; i < whitespaces; i++) Console.Write(" ");
                Console.WriteLine(text);
                Console.ForegroundColor = ConsoleColor.Gray;
                _Written = 0;
            }
            try
            {
                w.WriteLine(text);
            }
            catch { }
        }

        #region get-/setters

        /// <summary>
        /// The current time in the format HH.MM.SS.MMM
        /// </summary>
        public static string TimeString
        {
            get
            {
                DateTime now = DateTime.Now;
                string hour = (now.Hour > 9) ? now.Hour.ToString() : "0" + now.Hour;
                string minute = (now.Minute > 9) ? now.Minute.ToString() : "0" + now.Minute;
                string second = (now.Second > 9) ? now.Second.ToString() : "0" + now.Second;
                string millisecond = "";
                if (now.Millisecond < 10)
                {
                    millisecond = "00" + now.Millisecond;
                }
                else
                {
                    if (now.Millisecond < 100)
                    {
                        millisecond = "0" + now.Millisecond;
                    }
                    else
                    {
                        millisecond = now.Millisecond.ToString();
                    }
                }
                return hour + "." + minute + "." + second + "." + millisecond;
            }
        }

        /// <summary>
        /// A date string in the format YYYY.MM.DD - HH.MM.SS.MMM
        /// </summary>
        public static string DateTimeString
        {
            get
            {
                DateTime now = DateTime.Now;
                string year = now.Year.ToString();
                string month = (now.Month > 9) ? now.Month.ToString() : "0" + now.Month;
                string day = (now.Day > 9) ? now.Day.ToString() : "0" + now.Day;
                return year + "." + month + "." + day + " - " + TimeString;
            }
        }

        #endregion

        /// <summary>
        /// Extend a string with a number of spaces, so the resulting string
        /// will be max digits long.
        /// </summary>
        /// <param name="text">The text to extend</param>
        /// <param name="max">The number of digits the string should get extended to</param>
        /// <returns>The extended string</returns>
        public static string InsertSpaces(string text, int max)
        {
            if (text.Length >= max) return text;
            int len = text.Length;
            for (int i = 0; i < (max - len); i++)
            {
                text += " ";
            }
            return text;
        }

    }

}
