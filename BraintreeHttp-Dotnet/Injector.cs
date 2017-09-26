using System;
namespace BraintreeHttp
{
	public interface Injector
    {
        void Inject(HttpRequest request)
    }
}
