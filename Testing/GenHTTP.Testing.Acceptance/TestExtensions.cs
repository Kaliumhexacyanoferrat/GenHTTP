using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

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

        public static DateTime WithoutMS(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Kind);
        }

        public static void IgnoreSecurityErrors(this HttpWebRequest request)
        {
            request.ServerCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) =>
            {
                return true;
            };
        }

    }

}
