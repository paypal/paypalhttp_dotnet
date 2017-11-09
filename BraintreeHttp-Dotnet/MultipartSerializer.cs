using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace BraintreeHttp
{
    public class MultipartSerializer : ISerializer
    {
        public string GetContentTypeRegexPattern()
        {
            return "^multipart/.*$";
        }

        public object Decode(HttpContent content, Type responseType)
        {
            throw new IOException("Unable to deserialize Content-Type: multipart/form-data.");
        }

        public HttpContent Encode(HttpRequest request)
        {
            if (!(request.Body is IDictionary))
            {
                throw new IOException("Request requestBody must be Map<String, Object> when Content-Type is multipart/*");
            }

            MultipartFormDataContent form = new MultipartFormDataContent();
            var body = (Dictionary<string, object>)request.Body;

            foreach (KeyValuePair<string, object> item in body)
            {
                if (item.Value is FileStream)
                {
                    var file = (FileStream)item.Value;
                    MemoryStream memoryStream = new MemoryStream();
                    file.CopyTo(memoryStream);
                    var fileContent = new ByteArrayContent(memoryStream.ToArray());
                    fileContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                    {
                        FileName = file.Name,
                        Name = (string)item.Key
                    };
                    form.Add(fileContent);
                }
                else
                {
                    form.Add(new StringContent((string)item.Value), (string)item.Key);
                }
            }

            return form;
        }
    }
}
