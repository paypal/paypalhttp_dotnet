using System;
using System.Net.Http;

namespace PayPalHttp
{
    public class HttpRequest : HttpRequestMessage
    {
        public string Path              { get; set; }
        public object Body              { get; set; }
        public string ContentType       { get; set; }
        public string ContentEncoding   { get; set; }
        public Type ResponseType        { get; }

        public HttpRequest(string path, HttpMethod method, Type responseType)
        {
            Path = path;
            ResponseType = responseType;
            base.Method = method;
            ContentEncoding = "identity";
        }

        public HttpRequest(string path, HttpMethod method) : this(path, method, typeof(void)) {}

        public T Clone<T>() where T: HttpRequest
        {
            return (T) MemberwiseClone();
        }
	}
}
