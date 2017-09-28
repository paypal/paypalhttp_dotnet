using System;
using WireMock.Server;

namespace BraintreeHttp_Dotnet.Tests
{

	public class TestEnvironment : BraintreeHttp.Environment
	{

		public TestEnvironment(int port, bool useSSL = false)
		{
			this.port = port;
            this.useSSL = useSSL;
		}

		public int port;
        bool useSSL;

		public string BaseUrl()
        {
            var scheme = this.useSSL ? "https" : "http";
            return scheme + "://localhost:" + port;
		}
	}

    public class TestHarness: IDisposable
    {
        protected FluentMockServer server;

		public TestHarness()
        {
			server = FluentMockServer.Start();
			System.Threading.Thread.Sleep(200);
		}

    	public void Dispose()
    	{
    		server.Stop();
    	}

        protected BraintreeHttp.HttpClient Client()
        {
            return new BraintreeHttp.HttpClient(new TestEnvironment(server.Ports[0]));
        }
    }
}
