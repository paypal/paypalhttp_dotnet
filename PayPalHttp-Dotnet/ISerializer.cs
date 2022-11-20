using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace PayPalHttp
{
	public interface ISerializer
    {
        string GetContentTypeRegexPattern();
        Regex GetContentRegEx();
        HttpContent Encode(HttpRequest request);
        object Decode(HttpContent content, Type responseType);
    }
}
