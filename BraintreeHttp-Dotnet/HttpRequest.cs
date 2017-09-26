using System;
using System.Net.Http;

namespace BraintreeHttp
{
    public class HttpRequest: HttpRequestMessage
    {
        public string Path          { get; }
        public object Body          { get; set; }
        private Type ResponseType;

        public HttpRequest(string path, HttpMethod method) 
        {
            this.Path = path;
            base.Method = method;
        }
    }
}
