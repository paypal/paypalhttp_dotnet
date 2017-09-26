using System;
using WireMock.Server;

namespace BraintreeHttp_Dotnet.Tests
{

	public class TestEnvironment : BraintreeHttp.Environment
	{

		public TestEnvironment(int port)
		{
			this.port = port;
		}

		public int port;

		public string BaseUrl()
        {
			return "http://localhost:" + port;
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

    	protected BraintreeHttp.HttpClient client()
        {
            return new BraintreeHttp.HttpClient(new TestEnvironment(server.Ports[0]));
        }
    }
}
