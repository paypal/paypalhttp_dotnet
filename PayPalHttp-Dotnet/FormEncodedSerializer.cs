using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PayPalHttp
{
    public partial class FormEncodedSerializer: ISerializer
    {
        private const string RegExPattern = "application/x-www-form-urlencoded";
#if NET7_0_OR_GREATER
        private static readonly Regex _pattern = ContentTypeRegEx();
#else
        private static readonly Regex _pattern = new(RegExPattern, RegexOptions.Compiled);
#endif

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

#if NET7_0_OR_GREATER
        [GeneratedRegex(RegExPattern, RegexOptions.Compiled)]
        private static partial Regex ContentTypeRegEx();
#endif
    }
}
