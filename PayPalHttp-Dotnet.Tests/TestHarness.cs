using System;
using WireMock.Server;

namespace PayPalHttp.Tests
{

	public class TestEnvironment : PayPalHttp.IEnvironment
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
        protected WireMockServer server;

		public TestHarness()
        {
			server = WireMockServer.Start();
    	}

    	public void Dispose()
    	{
    		server.Stop();
    	}

        protected PayPalHttp.HttpClient Client()
        {
            return new PayPalHttp.HttpClient(new TestEnvironment(server.Ports[0]));
        }
    }
}
