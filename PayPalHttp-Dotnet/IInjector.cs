using System;
using System.Threading.Tasks;

namespace PayPalHttp
{
	public interface IInjector
    {
        Task<T> InjectAsync<T>(T request) where T: HttpRequest;
    }
}
