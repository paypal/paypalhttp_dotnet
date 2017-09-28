using System;
using System.Net.Http;
using System.Net.Http.Headers;
namespace BraintreeHttp
{
	public interface ISerializer
    {
        string GetContentTypeRegexPattern();
        HttpContent SerializeRequest(HttpRequest request);
        object DeserializeResponse(HttpContent content, Type responseType);
    }
}
