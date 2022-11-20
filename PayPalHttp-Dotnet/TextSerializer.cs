using System;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace PayPalHttp
{
    public class TextSerializer : ISerializer
    {
        private const string RegExPattern = "^text/.*$";
        private static readonly Regex _pattern = new Regex(RegExPattern, RegexOptions.Compiled);

        public object Decode(HttpContent content, Type responseType)
        {
            return content.ReadAsStringAsync().Result;
        }

        public HttpContent Encode(HttpRequest request)
        {
            return new StringContent(request.Body.ToString());
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
