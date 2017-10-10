using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;

namespace BraintreeHttp
{
    public class Encoder
    {
        private List<ISerializer> serializers;

        public Encoder()
        {
            serializers = new List<ISerializer>();
            RegisterSerializer(new JsonSerializer());
            RegisterSerializer(new TextSerializer());
            RegisterSerializer(new MultipartSerializer());
            RegisterSerializer(new FormEncodedSerializer());
        }

        public void RegisterSerializer(ISerializer serializer)
        {
            if (serializer != null)
            {
                serializers.Add(serializer);
            }
        }

        public HttpContent SerializeRequest(HttpRequest request)
        {
            if (request.ContentType == null)
            {
                throw new IOException("HttpRequest did not have content-type header set");
            }
            ISerializer serializer = GetSerializer(request.ContentType);
            if (serializer == null)
            {
                throw new IOException($"Unable to serialize request with Content-Type {request.ContentType}. Supported encodings are {GetSupportedContentTypes()}");
            }

            var content = serializer.SerializeRequest(request);

            return content;
        }

        public object DeserializeResponse(HttpContent content, Type responseType)
        {
            if (content.Headers.ContentType == null)
            {
                throw new IOException("HTTP response did not have content-type header set");
            }
            var contentType = content.Headers.ContentType.ToString();
            ISerializer serializer = GetSerializer(contentType);
            if (serializer == null)
            {
                throw new IOException($"Unable to deserialize response with Content-Type {contentType}. Supported encodings are {GetSupportedContentTypes()}");
            }

            return serializer.DeserializeResponse(content, responseType);
        }

        private ISerializer GetSerializer(string contentType)
        {
            foreach (var serializer in serializers)
            {
                Regex pattern = new Regex(serializer.GetContentTypeRegexPattern());
                if (pattern.Match(contentType).Success)
                {
                    return serializer;
                }
            }

            return null;
        }

        private string GetSupportedContentTypes()
        {
            List<string> contentTypes = new List<string>();
            foreach (var serializer in this.serializers)
            {
                contentTypes.Add(serializer.GetContentTypeRegexPattern());
            }

            return String.Join(", ", contentTypes);
        }
    }
}
