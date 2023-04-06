using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PayPalHttp
{
    public partial class MultipartSerializer : ISerializer
    {
        private const string RegExPattern = "^multipart/.*$";
#if NET7_0_OR_GREATER
        private static readonly Regex _pattern = ContentTypeRegEx();
#else
        private static readonly Regex _pattern = new(RegExPattern, RegexOptions.Compiled);
#endif
        public Task<object> DecodeAsync(HttpContent content, Type responseType)
        {
            throw new IOException($"Unable to deserialize Content-Type: multipart/form-data.");
        }

        private static string GetMimeMapping(string filename)
        {
            return Path.GetExtension(filename) switch
            {
                ".jpeg" => "image/jpeg",
                ".jpg" => "image/jpeg",
                ".gif" => "image/gif",
                ".png" => "image/png",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream",
            };
        }

        public async Task<HttpContent> EncodeAsync(HttpRequest request)
        {
            if (request.Body is not IDictionary)
            {
                throw new IOException("Request requestBody must be Map<String, Object> when Content-Type is multipart/*");
            }

            var boundary = "CustomBoundary8d0f01e6b3b5daf";
            MultipartFormDataContent form = new(boundary);
            var body = (Dictionary<string, object>)request.Body;

            foreach (KeyValuePair<string, object> item in body)
            {
                if (item.Value is FileStream file)
                {
                    try
                    {
                        MemoryStream memoryStream = new();
                        await file.CopyToAsync(memoryStream).ConfigureAwait(false);
                        var fileContent = new ByteArrayContent(memoryStream.ToArray());
                        var fileName = Path.GetFileName(file.Name);
                        // This is necessary to quote values since the web server is picky; .NET normally does not quote
                        fileContent.Headers.Add("Content-Disposition", "form-data; name=\"" + item.Key + "\"; filename=\"" + fileName + "\"");
                        string mimeType = GetMimeMapping(fileName);
                        fileContent.Headers.Add("Content-Type", mimeType);

                        form.Add(fileContent, item.Key);
                    }
                    finally
                    {
                        file.Dispose();
                    }
                }
                else if (item.Value is HttpContent httpContent)
                {
                    form.Add(httpContent, item.Key);
                }
                else
                {
                    form.Add(new StringContent((string)item.Value), item.Key);
                }
            }

            // This is necessary to avoid quoting the boundary value since the web server is picky; .NET may add quotes
            form.Headers.Remove("Content-Type");
            form.Headers.TryAddWithoutValidation("Content-Type", "multipart/form-data; boundary=" + boundary);
            return form;
        }

        public Regex GetContentRegEx()
        {
            return _pattern;
        }

        public string GetContentTypeRegexPattern()
        {
            return RegExPattern;
        }

#if NET7_0_OR_GREATER
        [GeneratedRegex(RegExPattern, RegexOptions.Compiled)]
        private static partial Regex ContentTypeRegEx();
#endif
    }
}
