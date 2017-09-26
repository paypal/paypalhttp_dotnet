using System;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
namespace BraintreeHttp
{
    public class HttpException: HttpRequestException
    {
        public HttpStatusCode StatusCode { get; }
		public HttpHeaders Headers { get; }

        public HttpException(HttpStatusCode statusCode, HttpHeaders headers, string message): base(message)
		{
            StatusCode = statusCode;
            Headers = headers;
    	}
    }
}
