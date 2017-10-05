using System;
using System.Net.Http;

namespace BraintreeHttp
{
    public class TextSerializer : ISerializer
    {

        public object DeserializeResponse(HttpContent content, Type responseType)
        {
            return content.ReadAsStringAsync().Result;
        }

        public string GetContentTypeRegexPattern()
        {
            return "^text/.*$";
        }

        public HttpContent SerializeRequest(HttpRequest request)
        {
            return new StringContent(request.Body.ToString());
        }
    }
}
