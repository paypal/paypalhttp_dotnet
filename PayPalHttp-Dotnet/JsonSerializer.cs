using System;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PayPalHttp
{
    public class JsonSerializer : ISerializer
    {
        private const string RegExPattern = "application/json";
        private static readonly Regex _pattern = new(RegExPattern, RegexOptions.Compiled);

        public async Task<object> DecodeAsync(HttpContent content, Type responseType)
        {
            var jsonSerializer = new DataContractJsonSerializer(responseType);            
            using (var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(await content.ReadAsStringAsync().ConfigureAwait(false))))
            {
                return jsonSerializer.ReadObject(ms);
            }
        }

        public async Task<HttpContent> EncodeAsync(HttpRequest request)
        {
            var jsonSerializer = new DataContractJsonSerializer(request.Body.GetType());

            using (var ms = new MemoryStream())
            {
                jsonSerializer.WriteObject(ms, request.Body);
                ms.Position = 0;
                using (var sr = new StreamReader(ms))
                {
                    return new StringContent(await sr.ReadToEndAsync().ConfigureAwait(false), System.Text.Encoding.UTF8, RegExPattern);
                }
            }
        }

        public Regex GetContentRegEx()
        {
            return _pattern;
        }

        public string GetContentTypeRegexPattern()
        {
            return RegExPattern;
        }
    }
}
