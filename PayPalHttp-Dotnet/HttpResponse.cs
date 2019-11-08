using System;
using System.Net;
using System.Net.Http.Headers;

namespace PayPalHttp
{
    public class HttpResponse
    {
        public HttpHeaders Headers          { get; }
    	public HttpStatusCode StatusCode    { get; }

        private object result;

        public HttpResponse(HttpHeaders headers, HttpStatusCode statusCode, object result)
        {
            this.Headers = headers;
            this.StatusCode = statusCode;
            this.result = result;
        }

        public T Result<T>()
        {
            return (T)this.result;
        }
    }
}
