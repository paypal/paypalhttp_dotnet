using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PayPalHttp
{
    public class FormEncodedSerializer: ISerializer
    {
        private const string RegExPattern = "application/x-www-form-urlencoded";
        private static readonly Regex _pattern = new(RegExPattern, RegexOptions.Compiled);

        public Task<object> DecodeAsync(HttpContent content, Type responseType)
        {
            throw new IOException($"Unable to deserialize Content-Type: {RegExPattern}.");
        }

        public async Task<HttpContent> EncodeAsync(HttpRequest request)
        {
            if (request.Body is not IDictionary)
            {
                throw new IOException("Request requestBody must be Map<string, string> when Content-Type is application/x-www-form-urlencoded");
            }

            return await Task.FromResult(new FormUrlEncodedContent((Dictionary<string, string>)request.Body)).ConfigureAwait(false);
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
