using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace BraintreeHttp
{
    public class HttpRequest : HttpRequestMessage, ICloneable
    {
        public string Path          { get; set; }
        public object Body          { get; set; }
        public string ContentType   { get; set; }
        public Type ResponseType    { get; }

        public HttpRequest(string path, HttpMethod method, Type responseType) 
        {
            this.Path = path;
            this.ResponseType = responseType;
            base.Method = method;
        }

        public HttpRequest(string path, HttpMethod method) : this(path, method, typeof(void)) {}

        public object Clone()
        {
            var other = new HttpRequest(this.Path, this.Method, this.ResponseType);
            other.ContentType = this.ContentType;
            other.Body = this.Body;

            foreach (var header in this.Headers)
            {
                other.Headers.Add(header.Key, header.Value);
            }
            return other;
        }
	}
}
