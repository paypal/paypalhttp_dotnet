using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;


namespace BraintreeHttp
{
    public class HttpClient
    {
        private Environment environment;
        private System.Net.Http.HttpClient client;
        private List<IInjector> injectors;
        private Encoder encoder;

        public HttpClient(Environment environment)
        {
            this.environment = environment;
            this.injectors = new List<IInjector>();
            this.encoder = new Encoder();

            client = new System.Net.Http.HttpClient();
            client.BaseAddress = new Uri(environment.BaseUrl());
            client.DefaultRequestHeaders.Add("User-Agent", GetUserAgent());
        }

        protected string GetUserAgent()
        {
            return "BraintreeHttp-Dotnet HTTP/1.1";
        }

        public void AddInjector(IInjector injector)
        {
            if (injector != null)
            {
                this.injectors.Add(injector);
            }
        }

        public void SetConnectTimeout(TimeSpan timeout)
        {
            client.Timeout = timeout;
        }

        public async Task<HttpResponse> Execute(HttpRequest request)
        {
            foreach (var injector in injectors) {
                injector.Inject(request);
            }

            request.RequestUri = new Uri(this.environment.BaseUrl() + request.Path);

            if (request.Body != null)
            {
                request.Content = this.SerializeRequest(request);
            }

			var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                object responseBody = null;
                if (response.Content.Headers.ContentType != null)
                {
                    responseBody = DeserializeResponse(response.Content, request.ResponseType);
                }
                return new HttpResponse(response.Headers, response.StatusCode, responseBody);
            }
            else
            {
				var responseBody = await response.Content.ReadAsStringAsync();
				throw new HttpException(response.StatusCode, response.Headers, responseBody);
            }
        }

        protected object DeserializeResponse(HttpContent content, Type responseType)
        {
            return encoder.DeserializeResponse(content, responseType);
        }

        protected HttpContent SerializeRequest(HttpRequest request)
        {
            return encoder.SerializeRequest(request);
		}
    }
}
