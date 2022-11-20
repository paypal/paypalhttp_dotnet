using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace PayPalHttp
{
    public class HttpClient
    {
        public Encoder Encoder { get; }

        protected Environment environment;
        private System.Net.Http.HttpClient client;
        private List<IInjector> injectors;

        public HttpClient(Environment environment)
        {
            this.environment = environment;
            this.injectors = new List<IInjector>();
            this.Encoder = new Encoder();

            client = new System.Net.Http.HttpClient();
            client.BaseAddress = new Uri(environment.BaseUrl());
            client.DefaultRequestHeaders.Add("User-Agent", GetUserAgent());
        }

        protected virtual string GetUserAgent()
        {
            return "PayPalHttp-Dotnet HTTP/1.1";
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

        public virtual async Task<HttpResponse> Execute<T>(T req) where T: HttpRequest
        {
            var request = req.Clone<T>();

            foreach (var injector in injectors) {
                injector.Inject(request);
            }

            request.RequestUri = new Uri(this.environment.BaseUrl() + request.Path);

            if (request.Body != null)
            {
                request.Content = await Encoder.SerializeRequestAsync(request);
            }

			var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                object responseBody = null;
                if (response.Content.Headers.ContentType != null)
                {
                    responseBody = await Encoder.DeserializeResponseAsync(response.Content, request.ResponseType);
                }
                return new HttpResponse(response.Headers, response.StatusCode, responseBody);
            }
            else
            {
				var responseBody = await response.Content.ReadAsStringAsync();
				throw new HttpException(response.StatusCode, response.Headers, responseBody);
            }
        }
    }
}
