using System;
using System.Net;
using System.Net.Http.Headers;

namespace BraintreeHttp
{
    public class HttpResponse
    {
        public HttpHeaders Headers          { get; }
    	public HttpStatusCode StatusCode    { get; }
		public object Result                     { get; }

        public HttpResponse(HttpHeaders headers, HttpStatusCode statusCode, object result)
        {
            this.Headers = headers;
            this.StatusCode = statusCode;
            this.Result = result;
        }
    }
}
