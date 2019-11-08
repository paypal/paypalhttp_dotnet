using System;

namespace PayPalHttp
{
	public interface IInjector
    {
        void Inject(HttpRequest request);
    }
}
