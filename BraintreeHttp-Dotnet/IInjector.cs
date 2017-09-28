using System;

namespace BraintreeHttp
{
	public interface IInjector
    {
        void Inject(HttpRequest request);
    }
}
