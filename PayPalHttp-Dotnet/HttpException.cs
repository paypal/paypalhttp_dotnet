using System.IO;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
namespace PayPalHttp
{
    public class HttpException: IOException
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
