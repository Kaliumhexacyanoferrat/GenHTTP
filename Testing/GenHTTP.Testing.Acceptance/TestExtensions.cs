using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace GenHTTP.Testing.Acceptance
{

    public static class TestExtensions
    {

        public static string GetContent(this HttpWebResponse response)
        {
            using var stream = response.GetResponseStream();
            using var reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }

        public static HttpWebResponse GetSafeResponse(this WebRequest request)
        {
            try
            {
                return (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                var response = e.Response as HttpWebResponse;

                if (response != null)
                {
                    return response;
                }
                else
                {
                    throw;
                }
            }
        }

    }

}
