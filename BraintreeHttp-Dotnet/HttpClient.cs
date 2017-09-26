using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.IO;


namespace BraintreeHttp
{
    public class HttpClient
    {
        private Environment Environment;
        private System.Net.Http.HttpClient Client;
        private List<Injector> Injectors;

        public HttpClient(Environment environment)
        {
            this.Environment = environment;
            this.Injectors = new List<Injector>();
            this.AddInjector(new UserAgentInjector(this.GetUserAgent()));

            Client = new System.Net.Http.HttpClient();
            Client.BaseAddress = new Uri(Environment.BaseUrl());
            Client.DefaultRequestHeaders.Add("User-Agent", GetUserAgent());
        }

        protected string GetUserAgent()
        {
            return "BraintreeHttp-Dotnet HTTP/1.1";
        }

        public void AddInjector(Injector injector)
        {
            if (injector != null)
            {
                this.Injectors.Add(injector);
            }
        }

        public async Task<HttpResponse> Execute(HttpRequest request)
        {
            request.RequestUri = new Uri(this.Environment.BaseUrl() + request.Path);

            if (request.Body != null)
            {
                request.Content = this.SerializeRequest(request);
            }

			var response = await Client.SendAsync(request);

    		var responseBody = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return new HttpResponse(response.Headers, response.StatusCode, "");
            }
            else
            {
                throw new HttpException(response.StatusCode, response.Headers, responseBody);
            }
        }

        //protected T DeserializeResponse<T>(HttpContent content)
        //{

        //}

        protected HttpContent SerializeRequest(HttpRequest request)
        {
            DataContractJsonSerializer jsonSer = new DataContractJsonSerializer(request.Body.GetType());

			MemoryStream ms = new MemoryStream();
            jsonSer.WriteObject(ms, request.Body);
			ms.Position = 0;

			StreamReader sr = new StreamReader(ms);
			StringContent theContent = new StringContent(sr.ReadToEnd(), System.Text.Encoding.UTF8, "application/json");

            return theContent;
		}

        private class UserAgentInjector: Injector
        {
            private string UserAgent;

            public UserAgentInjector(string UserAgent)
            {
                this.UserAgent = UserAgent;
            }

            public void Inject(HttpRequest request)
            {
            //    request.Headers.UserAgent = this.UserAgent;
            }
        }
    }
}
