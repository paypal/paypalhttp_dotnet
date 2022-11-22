using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PayPalHttp
{
    public class TextSerializer : ISerializer
    {
        private const string RegExPattern = "^text/.*$";
        private static readonly Regex _pattern = new(RegExPattern, RegexOptions.Compiled);

        public async Task<object> DecodeAsync(HttpContent content, Type responseType)
        {
            return await content.ReadAsStringAsync();
        }

        public async Task<HttpContent> EncodeAsync(HttpRequest request)
        {
            return await Task.FromResult(new StringContent(request.Body.ToString()));
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
