using System;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.IO;

namespace BraintreeHttp
{
    public class JsonSerializer: ISerializer
    {
        public string GetContentTypeRegexPattern()
        {
            return "application/json";
        }

        public object DeserializeResponse(HttpContent content, Type responseType)
        {
            var jsonSerializer = new DataContractJsonSerializer(responseType);

            using (var contentStream = content.ReadAsStreamAsync().Result) {
                return jsonSerializer.ReadObject(contentStream);
            }
        }

        public HttpContent SerializeRequest(HttpRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
