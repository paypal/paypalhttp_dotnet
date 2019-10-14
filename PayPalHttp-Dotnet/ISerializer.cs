using System;
using System.Net.Http;
using System.Net.Http.Headers;
namespace PayPalHttp
{
	public interface ISerializer
    {
        string GetContentTypeRegexPattern();
        HttpContent Encode(HttpRequest request);
        object Decode(HttpContent content, Type responseType);
    }
}
