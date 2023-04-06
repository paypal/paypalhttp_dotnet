using System.Net;
using System.Net.Http.Headers;

namespace PayPalHttp
{
    public class HttpResponse
    {
        public HttpHeaders Headers          { get; }
    	public HttpStatusCode StatusCode    { get; }

        private readonly object _result;

        public HttpResponse(HttpHeaders headers, HttpStatusCode statusCode, object result)
        {
            Headers = headers;
            StatusCode = statusCode;
            _result = result;
        }

        public T Result<T>()
        {
            return (T)_result;
        }
    }
}
