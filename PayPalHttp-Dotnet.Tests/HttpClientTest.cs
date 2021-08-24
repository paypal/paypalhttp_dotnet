using PayPalHttp;
using Xunit;
using System;
using System.Runtime.Serialization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Matchers;
using System.Collections.Generic;
using System.IO;

namespace PayPalHttp.Tests
{
    [DataContract]
    public class TestData
    {
        [DataMember(Name = "name")]
        public string Name;
    }

    class TestInjector : IInjector
    {
        public void Inject(HttpRequest request)
        {
            request.Headers.Add("User-Agent", "Custom Injector");
        }
    }

    class SimpleRequest: HttpRequest
    {
        public SimpleRequest(): base("/", HttpMethod.Post, typeof(void)) {}
    }

    public class HttpClientTest : TestHarness
    {
        [Fact]
        public async void Execute_throwsExceptionForNonSuccessfulStatusCodes()
        {
            server.Given(
                Request.Create().WithPath("/").UsingGet()
          ).RespondWith(
                Response.Create()
                    .WithStatusCode(400)
          );

            var request = new HttpRequest("/", HttpMethod.Get);

            try
            {
                await Client().Execute(request);
                Assert.True(false, "Expected client.Execute to throw HttpException");
            }
            catch (PayPalHttp.HttpException e)
            {
                Assert.Equal(System.Net.HttpStatusCode.BadRequest, e.StatusCode);
            }
        }

        [Fact]
        public async void Execute_returnsSuccessForSuccessfulStatusCodes()
        {
            server.Given(
                Request.Create().WithPath("/").UsingGet()
            ).RespondWith(
                Response.Create().WithStatusCode(200)
            );

            var request = new HttpRequest("/", HttpMethod.Get);

            var resp = await Client().Execute(request);
            Assert.Equal(System.Net.HttpStatusCode.OK, resp.StatusCode);
        }

        [Fact]
        public async void Execute_setsVerbFromRequest()
        {
            server.Given(
                Request.Create().WithPath("/").UsingDelete()
          ).RespondWith(
                Response.Create().WithStatusCode(204)
          );

            var request = new HttpRequest("/", HttpMethod.Delete);
            var resp = await Client().Execute(request);

            Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
            Assert.Equal("delete", GetLastRequest().RequestMessage.Method.ToLower());
        }

        [Fact]
        public async void Execute_UsesDefaultUserAgentHeader()
        {
            server.Given(
                Request.Create().WithPath("/").UsingGet()
            ).RespondWith(
                Response.Create().WithStatusCode(200)
            );

            var request = new HttpRequest("/", HttpMethod.Get);
            var resp = await Client().Execute(request);

            Assert.Equal("PayPalHttp-Dotnet HTTP/1.1", GetLastRequest().RequestMessage.Headers["User-Agent"]);
        }

        [Fact]
        public async void Execute_writesDataFromRequestIfPresent()
        {
            server.Given(
                Request.Create().WithPath("/")
                .UsingPost()
                .WithBody(@"some text here")
            ).RespondWith(
                Response.Create().WithStatusCode(200)
            );

            var request = new HttpRequest("/", HttpMethod.Post);
            request.Body = "some text here";
            request.ContentType = "text/plain";

            var response = await Client().Execute(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async void Execute_doesNotWriteDataFromRequestIfNotPresent()
        {
            server.Given(
                Request.Create().WithPath("/")
                .UsingPost()
            ).RespondWith(
                Response.Create().WithStatusCode(200)
            );

            var request = new HttpRequest("/", HttpMethod.Post);

            var response = await Client().Execute(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.Equal("", GetLastRequest().RequestMessage.Body);
        }

        [Fact]
        public async void Execute_doesNotMutateOriginalRequest()
        {
            server.Given(
                Request.Create().WithPath("/")
                .UsingPost()
            ).RespondWith(
                Response.Create().WithStatusCode(200)
            );

            var request = new SimpleRequest();

            var response = await Client().Execute(request);

            var headerCount = 0;
            foreach (var header in request.Headers)
            {
                headerCount++;
            }

            Assert.Equal(0, headerCount);
        }

        [Fact]
        public async void AddInjector_usesCustomInjectorsToModifyRequest()
        {
            server.Given(
                Request.Create().WithPath("/")
                .UsingGet()
            ).RespondWith(
                Response.Create().WithStatusCode(200)
            );

            var request = new HttpRequest("/", HttpMethod.Get);
            var client = Client();

            client.AddInjector(new TestInjector());

            var response = await client.Execute(request);
            Assert.Equal("Custom Injector", GetLastRequest().RequestMessage.Headers["User-Agent"]);
        }

        [Fact]
        public async void Execute_withData_SerializesDataAccordingToContentType()
        {
            server.Given(
                Request.Create().WithPath("/")
                .UsingPost()
                .WithBody("{\"name\":\"paypal\"}")
            ).RespondWith(
                Response.Create().WithStatusCode(200)
            );
            var request = new HttpRequest("/", HttpMethod.Post, typeof(void));
            request.ContentType = "application/json";
            request.Body = new TestData
            {
                Name = "paypal"
            };

            var client = Client();

            var response = await client.Execute(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async void Execute_withData_SerializesDataAccordingToContentTypeCaseInsensitive()
        {
            server.Given(
                Request.Create().WithPath("/")
                .UsingPost()
                .WithBody("{\"name\":\"paypal\"}")
            ).RespondWith(
                Response.Create().WithStatusCode(200)
            );
            var request = new HttpRequest("/", HttpMethod.Post, typeof(void));
            request.ContentType = "application/JSON";
            request.Body = new TestData
            {
                Name = "paypal"
            };

            var client = Client();

            var response = await client.Execute(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async void Execute_withReturnData_DeserializesAccordingToContentType()
        {
            server.Given(
                Request.Create().WithPath("/")
                .UsingGet()
            ).RespondWith(
                Response.Create().WithStatusCode(200)
                .WithBody("{\"name\":\"\"}")
                .WithBody("{\"name\":\"paypal\"}")
                .WithHeader("Content-Type", "application/json; charset=utf-8")
            );
            var request = new HttpRequest("/", HttpMethod.Get, typeof(TestData));

            var response = await Client().Execute(request);

            Assert.Equal("paypal", response.Result<TestData>().Name);
        }

        [Fact]
        public async void Execute_withReturnData_DeserializesAccordingToContentTypeCaseInsensitive()
        {
            server.Given(
                Request.Create().WithPath("/")
                .UsingGet()
            ).RespondWith(
                Response.Create().WithStatusCode(200)
                .WithBody("{\"name\":\"\"}")
                .WithBody("{\"name\":\"paypal\"}")
                .WithHeader("Content-Type", "application/JSON; charset=utf-8")
            );
            var request = new HttpRequest("/", HttpMethod.Get, typeof(TestData));

            var response = await Client().Execute(request);

            Assert.Equal("paypal", response.Result<TestData>().Name);
        }

        [Fact]
        public async void AddInjector_withNull_doesNotThrow()
        {
            server.Given(
                Request.Create().WithPath("/")
                .UsingGet()
            ).RespondWith(
                Response.Create().WithStatusCode(200)
            );

            var request = new HttpRequest("/", HttpMethod.Get);
            var client = Client();

            client.AddInjector(null);

            await client.Execute(request);
        }

        private WireMock.Logging.LogEntry GetLastRequest()
        {
            foreach (var log in server.LogEntries)
            {
                return log;
            }

            return null;
        }
    }
}
