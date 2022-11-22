using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace PayPalHttp
{
    public class HttpClient
    {               
        private readonly System.Net.Http.HttpClient _client;
        private readonly List<IInjector> _injectors = new();
        protected TimeSpan _timeout = TimeSpan.FromMinutes(5); //5 minute http pool default timeout
        protected readonly Environment _environment;

        private static readonly ConcurrentDictionary<string, System.Net.Http.HttpClient> ClientDictionary = new();

        public Encoder Encoder { get; private set; }


        public HttpClient(Environment environment)
        {
            _environment = environment;
            Encoder = new Encoder();
#if NET6_0_OR_GREATER
            _client = GetHttpClient(environment.BaseUrl());
#else
            _client = new System.Net.Http.HttpClient();
            _client.BaseAddress = new Uri(environment.BaseUrl());
            _client.DefaultRequestHeaders.Add("User-Agent", GetUserAgent());
#endif
        }

#if NET6_0_OR_GREATER
        protected virtual SocketsHttpHandler GetHttpSocketHandler()
        {
            return new SocketsHttpHandler() {  PooledConnectionLifetime = _timeout };
        }

        protected virtual System.Net.Http.HttpClient GetHttpClient(string baseUrl)
        {
            return ClientDictionary.GetOrAdd(baseUrl.ToLower(), (bUrl) => {
                var client = new System.Net.Http.HttpClient(GetHttpSocketHandler())
                {
                    BaseAddress = new Uri(baseUrl)
                };
                client.DefaultRequestHeaders.Add("User-Agent", GetUserAgent());

                return client;
            });
        }
#endif
        protected virtual string GetUserAgent()
        {
            return "PayPalHttp-Dotnet HTTP/1.1";
        }

        public void AddInjector(IInjector injector)
        {
            if (injector != null)
            {
                _injectors.Add(injector);
            }
        }

        public void SetConnectTimeout(TimeSpan timeout)
        {
            _client.Timeout = _timeout = timeout;
        }

        public virtual async Task<HttpResponse> Execute<T>(T req) where T: HttpRequest
        {
            var request = req.Clone<T>();

            foreach (var injector in _injectors) {
                request = await injector.InjectAsync(request);
            }

            request.RequestUri = new Uri(_environment.BaseUrl() + request.Path);

            if (request.Body != null)
            {
                request.Content = await Encoder.SerializeRequestAsync(request);
            }

			var response = await _client.SendAsync(request);

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
