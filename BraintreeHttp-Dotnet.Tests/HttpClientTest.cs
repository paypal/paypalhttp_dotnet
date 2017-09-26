using Xunit;
using System.Runtime.Serialization;
using System.Net;
using System.Net.Http;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Matchers;

namespace BraintreeHttp_Dotnet.Tests
{

    [DataContract]
    public class TestData
    {
        [DataMember(Name = "name")]
        public string Name;
    }

    public class HttpClientTest : TestHarness
    {

        [Fact]
        public async void TestHttpClient_execute_throwsExceptionForNonSuccessfulStatusCodes()
        {
            server
              .Given(
                Request.Create().WithPath("/").UsingGet()
              )
              .RespondWith(
                Response.Create()
                  .WithStatusCode(400)
              );

            var request = new BraintreeHttp.HttpRequest("/", HttpMethod.Get);

            try
            {
                await client().Execute(request);
                Assert.True(false, "Expected client.Execute to throw HttpException");
            }
            catch (BraintreeHttp.HttpException e)
            {
                Assert.Equal(System.Net.HttpStatusCode.BadRequest, e.StatusCode);
            }
        }

        [Fact]
        public async void TestHttpClient_execute_returnsSuccess()
        {
            server
              .Given(
                Request.Create().WithPath("/").UsingGet()
              )
              .RespondWith(
                Response.Create()
                  .WithStatusCode(200)
              );

            var request = new BraintreeHttp.HttpRequest("/", HttpMethod.Get);

            var resp = await client().Execute(request);
            Assert.Equal(System.Net.HttpStatusCode.OK, resp.StatusCode);
        }

        [Fact]
        public async void TestHttpClient_execute_setsVerbFromRequest()
        {
            server
              .Given(
                    Request.Create().WithPath("/").UsingDelete()
              )
              .RespondWith(
                Response.Create()
                  .WithStatusCode(204)
              );

            var request = new BraintreeHttp.HttpRequest("/", HttpMethod.Delete);
            var resp = await client().Execute(request);

            Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
            Assert.Equal("delete", GetLastRequest().RequestMessage.Method.ToLower());
        }

        [Fact]
        public async void testHttpClient_Execute_UsesDefaultUserAgentHeader()
        {
            server
            .Given(
                    Request.Create().WithPath("/").UsingGet()
                )
                .RespondWith(Response.Create().WithStatusCode(200));

            var request = new BraintreeHttp.HttpRequest("/", HttpMethod.Get);
            var resp = await client().Execute(request);

            Assert.Equal("BraintreeHttp-Dotnet HTTP/1.1", GetLastRequest().RequestMessage.Headers["User-Agent"]);
        }

        [Fact]
        public async void TestHttpClient_execute_SSL()
        {
            server = WireMock.Server.FluentMockServer.Start(new WireMock.Settings.FluentMockServerSettings()
            {
                UseSSL = true
            });
            System.Threading.Thread.Sleep(200);

			server
			 .Given(
			   Request.Create().WithPath("/").UsingGet()
			 )
			 .RespondWith(
			   Response.Create()
				 .WithStatusCode(200)
			 );

            var sslClient = new BraintreeHttp.HttpClient(new TestEnvironment(server.Ports[0], true));

			var request = new BraintreeHttp.HttpRequest("/", HttpMethod.Get);

			var resp = await sslClient.Execute(request);
            Assert.Equal(System.Net.HttpStatusCode.OK, resp.StatusCode);
        }

        [Fact]
        public void TestHttpClient_Execute_RespectsConnectTimeout()
        {

        }

        [Fact]
        public void TestHttpClient_execute_writesDataFromRequestIfPresent()
        {

        }

        [Fact]
        public void TestHttpClient_execute_doesNotwriteDataFromRequestIfNotPresent()
        {

        }

        [Fact]
        public void TestHttpClient_addInjector_usesCustomInjectors()
        {
        }

        [Fact]
        public void TestHttpClient_addInjector_withNull_doestNotAddNullInjector()
        {
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
