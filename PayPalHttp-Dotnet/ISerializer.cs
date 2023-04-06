using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PayPalHttp
{
    public interface ISerializer
    {
        string GetContentTypeRegexPattern();
        Regex GetContentRegEx();
        Task<HttpContent> EncodeAsync(HttpRequest request);
        Task<object> DecodeAsync(HttpContent content, Type responseType);
    }
}
