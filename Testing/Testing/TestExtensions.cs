using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace GenHTTP.Testing
{
    
    public static class TestExtensions
    {

        public static async ValueTask<string> GetContent(this HttpResponseMessage response) => await response.Content.ReadAsStringAsync();

        public static string? GetHeader(this HttpResponseMessage response, string key)
        {
            if (response.Headers.TryGetValues(key, out var values))
            {
                return values.FirstOrDefault();
            }

            return null;
        }

        public static string? GetContentHeader(this HttpResponseMessage response, string key)
        {
            if (response.Content.Headers.TryGetValues(key, out var values))
            {
                return values.FirstOrDefault();
            }

            return null;
        }

    }

}
