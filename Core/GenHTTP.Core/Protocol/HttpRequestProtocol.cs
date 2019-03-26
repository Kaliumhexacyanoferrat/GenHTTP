using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Core.Protocol
{

    // ToDo: Rework this
    //       Requestbuilder?

    internal static class HttpRequestProtocol
    {

        internal static void ParseType(this HttpRequest request, string type)
        {
            if (type == "POST") { request.Type = RequestType.POST; return; }
            if (type == "HEAD") { request.Type = RequestType.HEAD; return; }

            request.Type = RequestType.GET;
        }

        internal static void ParseHttp(this HttpRequest request, string version)
        {
            if (version == "1.0") request.ProtocolType = ProtocolType.Http_1_0;
            request.ProtocolType = ProtocolType.Http_1_1;
        }

        internal static void ParseURL(this HttpRequest request, string URL)
        {
            request.Path = URL;

            // read GET parameters
            int pos = request.Path.IndexOf('?');

            if (pos > -1)
            {
                string getPart = (request.Path.Length > pos) ? request.Path.Substring(pos + 1) : "";

                foreach (Match m in Pattern.GET_PARAMETER.Matches(getPart))
                {
                    // add this get parameter only, if it does not exist yet
                    if (!request.Query.ContainsKey(m.Groups[1].Value))
                        request.Query.Add(m.Groups[1].Value, Uri.UnescapeDataString(m.Groups[2].Value.Replace('+', ' ')));
                }

                request.Path = request.Path.Substring(0, pos);
            }
        }

        internal static void ParseHeaderField(this HttpRequest request, string field, string value)
        {
            if (field.ToLower() == "cookie")
            {
                string[] cookies = value.Split("; ".ToCharArray());
                foreach (string cookie in cookies)
                {
                    int pos = cookie.IndexOf("=");
                    if (pos > -1)
                    {
                        string name = cookie.Substring(0, pos);
                        string val = cookie.Substring(pos + 1);
                        if (!request.Cookies.Exists(name)) request.Cookies.Add(name, val);
                    }
                }
            }
            else
            {
                if (!request.Additional.ContainsKey(field.ToLower())) request.Additional.Add(field.ToLower(), value);
            }
        }

        internal static void ParseBody(this HttpRequest request, string body)
        {
            if (request.Type == RequestType.POST)
            {
                string[] fields = body.Split("&".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                foreach (string field in fields)
                {
                    int pos = field.IndexOf("=");
                    if (pos > -1)
                    {
                        string name = field.Substring(0, pos);
                        string value = field.Substring(pos + 1);
                        // add, if there is no field with this name
                        if (!request.PostFields.ContainsKey(name)) request.PostFields.Add(name, DecodeData(value));
                    }
                }
            }
        }

        /// <summary>
        /// Decode hexadecimal encoded data (e.g. %20).
        /// </summary>
        /// <param name="toDecode">The string to decode</param>
        /// <returns>The decoded string</returns>
        internal static string DecodeData(string toDecode)
        {
            toDecode = toDecode.Replace("+", " ");
            StringBuilder ret = new StringBuilder();
            for (int i = 0; i < toDecode.Length; i++)
            {
                // current entity does not start with a %
                if (!(toDecode[i] == '%')) { ret.Append(toDecode[i]); continue; }
                // maybe got a hexadecimal entity => read the length
                if (IsHex(toDecode[i + 1]) && IsHex(toDecode[i + 2]))
                {
                    ret.Append(Char.ConvertFromUtf32(Convert.ToInt32(toDecode.Substring(i + 1, 2), 16)));
                    // don't forget that we did some look forward
                    i += 2;
                }
                else
                {
                    ret.Append("%");
                }
            }
            return ret.ToString();
        }

        internal static bool IsHex(char c)
        {
            return (c >= '0' && c <= '9') ||
                   (c >= 'a' && c <= 'F') ||
                   (c >= 'A' && c <= 'F');
        }

    }

}
