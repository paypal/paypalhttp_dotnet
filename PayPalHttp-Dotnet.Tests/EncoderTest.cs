using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Xunit;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace PayPalHttp.Tests
{
    public class EncoderTest
    {
        [Fact]
        public async Task SerializeRequest_throwsForUnsupportedContentType()
        {
            var request = new HttpRequest("/", HttpMethod.Get);
            request.ContentType = "application/not-supported";

            var encoder = new Encoder();
            try
            {
                _ = await encoder.SerializeRequestAsync(request).ConfigureAwait(false);
                Assert.True(false, "Serialize request did not throw an IOException");
            }
            catch (System.IO.IOException e)
            {
                Assert.StartsWith("Unable to serialize request with Content-Type application/not-supported. Supported encodings are", e.Message);
            }
        }

        [Fact]
        public async Task SerializeRequest_throwsWhenContentTypeNotPresent()
        {
            var request = new HttpRequest("/", HttpMethod.Get);

            var encoder = new Encoder();
            try
            {
                _ = await encoder.SerializeRequestAsync(request).ConfigureAwait(false);
                Assert.True(false, "Serialize request did not throw an IOException");
            }
            catch (System.IO.IOException e)
            {
                Assert.StartsWith("HttpRequest did not have content-type header set", e.Message);
            }
        }

        [Fact]
        public async Task SerializeRequest_withJsonContentTypeAsync()
        {
            var request = new HttpRequest("/", HttpMethod.Get);
            request.ContentType = "application/json";
            request.Body = new TestData
            {
                Name = "paypal"
            };

            var encoder = new Encoder();
            var content = await encoder.SerializeRequestAsync(request).ConfigureAwait(false);
            Assert.StartsWith("application/json", content.Headers.ContentType.ToString());

            var jsonString = await content.ReadAsStringAsync().ConfigureAwait(false);

            Assert.Equal("{\"name\":\"paypal\"}", jsonString);
        }

        [Fact]
        public async Task SerializeRequest_withJsonContentTypeAsyncCaseInsensitive()
        {
            var request = new HttpRequest("/", HttpMethod.Get);
            request.ContentType = "application/JSON";
            request.Body = new TestData
            {
                Name = "paypal"
            };

            var encoder = new Encoder();
            var content = await encoder.SerializeRequestAsync(request).ConfigureAwait(false);
            Assert.StartsWith("application/json", content.Headers.ContentType.ToString());

            var jsonString = await content.ReadAsStringAsync().ConfigureAwait(false);

            Assert.Equal("{\"name\":\"paypal\"}", jsonString);
        }

        [Fact]
        public async Task SerializeRequest_withMultipartContentTypeAsync()
        {
            var request = new HttpRequest("/", HttpMethod.Get);
            request.ContentType = "multipart/form-data";
            request.Body = new Dictionary<string, object>()
            {
                {"hello", "world"},
                {"something", "Else"},
                {"myfile", File.Open("../../../../README.md", FileMode.Open)}
            };

            var encoder = new Encoder();
            var content = await encoder.SerializeRequestAsync(request).ConfigureAwait(false);
            Assert.StartsWith("multipart/form-data; boundary=", content.Headers.ContentType.ToString());
            Assert.DoesNotContain("\"", content.Headers.ContentType.ToString());
        }

        [Fact]
        public async Task SerializeRequest_withMultipartContentTypeAsyncCaseInsensitive()
        {
            var request = new HttpRequest("/", HttpMethod.Get);
            request.ContentType = "MULTIPART/form-data";
            request.Body = new Dictionary<string, object>()
            {
                {"hello", "world"},
                {"something", "Else"},
                {"myfile", File.Open("../../../../README.md", FileMode.Open)}
            };

            var encoder = new Encoder();
            var content = await encoder.SerializeRequestAsync(request).ConfigureAwait(false);
            Assert.StartsWith("multipart/form-data; boundary=", content.Headers.ContentType.ToString());
            Assert.DoesNotContain("\"", content.Headers.ContentType.ToString());
        }

        [Fact]
        public async Task SerializeRequest_withMultipartContentTypeAndHttpContentTypes()
        {
            var inputJSON = "{\"key\":\"val\"}";
            var inputStringContent = new StringContent(inputJSON);
            inputStringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            inputStringContent.Headers.Add("Content-Disposition", "form-data; name=\"input\"; filename=\"input.json\"");

            var request = new HttpRequest("/", HttpMethod.Get);
            request.ContentType = "multipart/form-data";
            request.Body = new Dictionary<string, object>()
            {
                {"input_key", inputStringContent},
                {"myfile", File.Open("../../../../README.md", FileMode.Open)}
            };

            var encoder = new Encoder();
            var content = await encoder.SerializeRequestAsync(request).ConfigureAwait(false);

            var body = await content.ReadAsStringAsync().ConfigureAwait(false);
            Assert.Contains("{\"key\":\"val\"}", body);
            Assert.Contains("Content-Type: application/json", body);
            Assert.Contains("Content-Disposition: form-data; name=\"input\"; filename=\"input.json\"", body);
            Assert.StartsWith("multipart/form-data; boundary=", content.Headers.ContentType.ToString());
            Assert.DoesNotContain("\"", content.Headers.ContentType.ToString());
        }

        [Fact]
        public async Task SerializeRequest_withMultipartContentTypeAndJsonPartContent()
        {
            var inputJSON = new TestData
            {
                Name = "paypal"
            };
            var jsonPart = new JsonPartContent("input", inputJSON);

            var request = new HttpRequest("/", HttpMethod.Get);
            request.ContentType = "multipart/form-data";
            request.Body = new Dictionary<string, object>()
            {
                {"input_key", jsonPart},
                {"myfile", File.Open("../../../../README.md", FileMode.Open)}
            };

            var encoder = new Encoder();
            var content = await encoder.SerializeRequestAsync(request).ConfigureAwait(false);

            var body = await content.ReadAsStringAsync().ConfigureAwait(false);
            Assert.Contains("{\"name\":\"paypal\"}", body);
            Assert.Contains("Content-Type: application/json", body);
            Assert.Contains("Content-Disposition: form-data; name=\"input\"; filename=\"input.json\"", body);
            Assert.StartsWith("multipart/form-data; boundary=", content.Headers.ContentType.ToString());
            Assert.DoesNotContain("\"", content.Headers.ContentType.ToString());
        }

        [Fact]
        public async Task SerializeRequest_withTextContentTypeAsync()
        {
            var request = new HttpRequest("/", HttpMethod.Get);
            request.ContentType = "text/plain";
            request.Body = "some plain text";

            var encoder = new Encoder();
            var content = await encoder.SerializeRequestAsync(request).ConfigureAwait(false);
            Assert.StartsWith("text/plain", content.Headers.ContentType.ToString());

            var textString = await content.ReadAsStringAsync().ConfigureAwait(false);
            Assert.Equal("some plain text", textString);
        }

        [Fact]
        public async Task SerializeReqeust_withFormEncodedContentType()
        {
            var request = new HttpRequest("/", HttpMethod.Get);
            request.ContentType = "application/x-www-form-urlencoded";
            request.Body = new Dictionary<string, string>()
            {
                {"hello", "world"},
                {"key", "value"},
                {"another_key", "some value with spaces"},
            };

            var encoder = new Encoder();
            var content = await encoder.SerializeRequestAsync(request).ConfigureAwait(false);
            Assert.StartsWith("application/x-www-form-urlencoded", content.Headers.ContentType.ToString());

            var textString = await content.ReadAsStringAsync().ConfigureAwait(false);
            Assert.Equal("hello=world&key=value&another_key=some+value+with+spaces", textString);
        }

        [Fact]
        public async Task SerializeRequest_withGzipContentEncoding()
        {
            var encoder = new Encoder();
            var request = new HttpRequest("/", HttpMethod.Get);

            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentEncoding = "gzip";
            request.Body = new Dictionary<string, string>()
            {
                {"hello", "world"},
                {"key", "value"},
                {"another_key", "some value with spaces"},
            };

            var content = await encoder.SerializeRequestAsync(request).ConfigureAwait(false);
            var buf = await content.ReadAsByteArrayAsync().ConfigureAwait(false);

            Assert.Equal(await GzipAsync("hello=world&key=value&another_key=some+value+with+spaces"), buf);
        }

        [Fact]
        public async Task DeserializeResponse_throwsForUnsupportedContentType()
        {
            var responseContent = new StringContent("some data", Encoding.UTF8, "application/unsupported");

            var encoder = new Encoder();
            try
            {
                var content = await encoder.DeserializeResponseAsync(responseContent, typeof(String)).ConfigureAwait(false);
                Assert.True(false, "Should throw IOException with unsupported content type");
            }
            catch (System.IO.IOException ex)
            {
                Assert.StartsWith("Unable to deserialize response with Content-Type application/unsupported", ex.Message);
            }
        }

        [Fact]
        public async Task DeserializeResponse_throwsWhenContentTypeNotPresent()
        {
            var responseContent = new StringContent("some data");
            responseContent.Headers.ContentType = null;

            var encoder = new Encoder();
            try
            {
                var content = await encoder.DeserializeResponseAsync(responseContent, typeof(String)).ConfigureAwait(false);
                Assert.True(false, "Should throw IOException with missing content type header");
            }
            catch (System.IO.IOException ex)
            {
                Assert.Equal("HTTP response did not have content-type header set", ex.Message);
            }
        }

        [Fact]
        public async Task DeserializeResponse_withJsonContentType()
        {
            var responseContent = new StringContent("{\"name\":\"paypal\"}", Encoding.UTF8, "application/json");

            var encoder = new Encoder();
            var content = await encoder.DeserializeResponseAsync(responseContent, typeof(TestData)).ConfigureAwait(false);

            Assert.NotNull(content);
            Assert.Equal("paypal", ((TestData)content).Name);
        }

        [Fact]
        public async Task DeserializeResponse_withJsonContentTypeCaseInsensitive()
        {
            var responseContent = new StringContent("{\"name\":\"paypal\"}", Encoding.UTF8, "application/JSON");

            var encoder = new Encoder();
            var content = await encoder.DeserializeResponseAsync(responseContent, typeof(TestData)).ConfigureAwait(false);

            Assert.NotNull(content);
            Assert.Equal("paypal", ((TestData)content).Name);
        }

        [Fact]
        public async Task DeserializeResponse_withTextContentType()
        {
            var responseContent = new StringContent("some plain text", Encoding.UTF8, "text/plain");

            var encoder = new Encoder();
            var content = await encoder.DeserializeResponseAsync(responseContent, typeof(String)).ConfigureAwait(false);

            Assert.NotNull(content);
            Assert.Equal("some plain text", content);
        }

        [Fact]
        public async Task DeserializeResponse_withTextContentTypeCaseInsensitive()
        {
            var responseContent = new StringContent("some plain text", Encoding.UTF8, "text/PLAIN");

            var encoder = new Encoder();
            var content = await encoder.DeserializeResponseAsync(responseContent, typeof(String)).ConfigureAwait(false);

            Assert.NotNull(content);
            Assert.Equal("some plain text", content);
        }

        [Fact]
        public async Task DeserializeResponse_throwsForMultipartContentType()
        {
            var responseContent = new StringContent("some data", Encoding.UTF8, "multipart/form-data");

            var encoder = new Encoder();
            try
            {
                var content = await encoder.DeserializeResponseAsync(responseContent, typeof(String)).ConfigureAwait(false);
                Assert.True(false, "We do not deserialize multipart data");
            }
            catch (System.IO.IOException ex)
            {
                Assert.Equal("Unable to deserialize Content-Type: multipart/form-data.", ex.Message);
            }
        }

        [Fact]
        public async Task DeserializeResponse_throwsForFormEncodedContentType()
        {
            var responseContent = new StringContent("hello=world", Encoding.UTF8, "application/x-www-form-urlencoded");

            var encoder = new Encoder();
            try
            {
                var content = await encoder.DeserializeResponseAsync(responseContent, typeof(String)).ConfigureAwait(false);
                Assert.True(false, "form encoded deserialization not supported");
            }
            catch (System.IO.IOException ex)
            {
                Assert.Equal("Unable to deserialize Content-Type: application/x-www-form-urlencoded.", ex.Message);
            }
        }

        [Fact]
        public async Task DeserializeResponse_withGzipEncoding()
        {
            var encoder = new Encoder();

            var content = "hello world";
            var responseContent = new ByteArrayContent(await GzipAsync(content));

            responseContent.Headers.Add("Content-Type", "text/plain");
            responseContent.Headers.ContentEncoding.Add("gzip");

            var deserializedContent = await encoder.DeserializeResponseAsync(responseContent, typeof(string)).ConfigureAwait(false);

            Assert.Equal(content, deserializedContent);
        }

        private static async Task<byte[]> GzipAsync(string source)
        {
            var bytes = Encoding.UTF8.GetBytes(source);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    await msi.CopyToAsync(gs).ConfigureAwait(false);
                }

                return mso.ToArray();
            }
        }
    }
}
