using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace PayPalHttp
{
    public class FormEncodedSerializer: ISerializer
    {
        private const string RegExPattern = "application/x-www-form-urlencoded";
        private static readonly Regex _pattern = new Regex(RegExPattern, RegexOptions.Compiled);

        public object Decode(HttpContent content, Type responseType)
        {
            throw new IOException($"Unable to deserialize Content-Type: {RegExPattern}.");
        }

        public HttpContent Encode(HttpRequest request)
        {
            if (!(request.Body is IDictionary))
            {
                throw new IOException("Request requestBody must be Map<string, string> when Content-Type is application/x-www-form-urlencoded");
            }

            return new FormUrlEncodedContent((Dictionary<string, string>)request.Body);
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
