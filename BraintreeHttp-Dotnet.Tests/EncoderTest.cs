using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Xunit;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace BraintreeHttp.Tests
{
    public class EncoderTest
    {
        [Fact]
        public void SerializeRequest_throwsForUnsupportedContentType()
        {
            var request = new HttpRequest("/", HttpMethod.Get);
            request.ContentType = "application/not-supported";

            var encoder = new Encoder();
            try
            {
                encoder.SerializeRequest(request);
                Assert.True(false, "Serialize request did not throw an IOException");
            }
            catch (System.IO.IOException e)
            {
                Assert.StartsWith("Unable to serialize request with Content-Type application/not-supported. Supported encodings are", e.Message);
            }
        }

        [Fact]
        public void SerializeRequest_throwsWhenContentTypeNotPresent()
        {
            var request = new HttpRequest("/", HttpMethod.Get);

            var encoder = new Encoder();
            try
            {
                encoder.SerializeRequest(request);
                Assert.True(false, "Serialize request did not throw an IOException");
            }
            catch (System.IO.IOException e)
            {
                Assert.StartsWith("HttpRequest did not have content-type header set", e.Message);
            }
        }

        [Fact]
        public async void SerializeRequest_withJsonContentTypeAsync()
        {
            var request = new HttpRequest("/", HttpMethod.Get);
            request.ContentType = "application/json";
            request.Body = new TestData
            {
                Name = "braintree"
            };

            var encoder = new Encoder();
            var content = encoder.SerializeRequest(request);
            Assert.StartsWith("application/json", content.Headers.ContentType.ToString());

            var jsonString = await content.ReadAsStringAsync();

            Assert.Equal("{\"name\":\"braintree\"}", jsonString);
        }

        [Fact]
        public void SerializeRequest_withMultipartContentTypeAsync()
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
            var content = encoder.SerializeRequest(request);
            Assert.StartsWith("multipart/form-data; boundary=", content.Headers.ContentType.ToString());
        }

        [Fact]
        public async void SerializeRequest_withMultipartContentTypeAndHttpContentTypes()
        {
            var inputJSON = "{\"key\":\"val\"}";
            var inputStringContent = new StringContent(inputJSON);
            inputStringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            inputStringContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
            {
                FileName = "input.json",
                Name = "input"
            };

            var request = new HttpRequest("/", HttpMethod.Get);
            request.ContentType = "multipart/form-data";
            request.Body = new Dictionary<string, object>()
            {
                {"input_key", inputStringContent},
                {"myfile", File.Open("../../../../README.md", FileMode.Open)}
            };

            var encoder = new Encoder();
            var content = encoder.SerializeRequest(request);

            var body = await content.ReadAsStringAsync();
            Assert.Contains("{\"key\":\"val\"}", body);
            Assert.Contains("Content-Type: application/json", body);
            Assert.Contains("Content-Disposition: form-data; filename=input.json; name=input", body);
            Assert.StartsWith("multipart/form-data; boundary=", content.Headers.ContentType.ToString());
        }

        [Fact]
        public async void SerializeRequest_withMultipartContentTypeAndJsonPartContent()
        {
            var inputJSON = new TestData
            {
                Name = "braintree"
            };
            var jsonPart = new JsonPartContent("input with space", inputJSON);

            var request = new HttpRequest("/", HttpMethod.Get);
            request.ContentType = "multipart/form-data";
            request.Body = new Dictionary<string, object>()
            {
                {"input_key", jsonPart},
                {"myfile", File.Open("../../../../README.md", FileMode.Open)}
            };

            var encoder = new Encoder();
            var content = encoder.SerializeRequest(request);

            var body = await content.ReadAsStringAsync();
            Assert.Contains("{\"name\":\"braintree\"}", body);
            Assert.Contains("Content-Type: application/json", body);
            Assert.Contains("Content-Disposition: form-data; filename=\"input with space.json\"; name=\"input with space\"", body);
            Assert.StartsWith("multipart/form-data; boundary=", content.Headers.ContentType.ToString());
        }

        [Fact]
        public async void SerializeRequest_withTextContentTypeAsync()
        {
            var request = new HttpRequest("/", HttpMethod.Get);
            request.ContentType = "text/plain";
            request.Body = "some plain text";

            var encoder = new Encoder();
            var content = encoder.SerializeRequest(request);
            Assert.StartsWith("text/plain", content.Headers.ContentType.ToString());

            var textString = await content.ReadAsStringAsync();
            Assert.Equal("some plain text", textString);
        }

        [Fact]
        public async void SerializeReqeust_withFormEncodedContentType()
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
            var content = encoder.SerializeRequest(request);
            Assert.StartsWith("application/x-www-form-urlencoded", content.Headers.ContentType.ToString());

            var textString = await content.ReadAsStringAsync();
            Assert.Equal("hello=world&key=value&another_key=some+value+with+spaces", textString);
        }

        [Fact]
        public void SerializeRequest_withGzipContentEncoding()
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

            var content = encoder.SerializeRequest(request);
            var buf = content.ReadAsByteArrayAsync().Result;

            Assert.Equal(Gzip("hello=world&key=value&another_key=some+value+with+spaces"), buf);
        }

        [Fact]
        public void DeserializeResponse_throwsForUnsupportedContentType()
        {
            var responseContent = new StringContent("some data", Encoding.UTF8, "application/unsupported");

            var encoder = new Encoder();
            try
            {
                var content = encoder.DeserializeResponse(responseContent, typeof(String));
                Assert.True(false, "Should throw IOException with unsupported content type");
            }
            catch (System.IO.IOException ex)
            {
                Assert.StartsWith("Unable to deserialize response with Content-Type application/unsupported", ex.Message);
            }
        }

        [Fact]
        public void DeserializeResponse_throwsWhenContentTypeNotPresent()
        {
            var responseContent = new StringContent("some data");
            responseContent.Headers.ContentType = null;

            var encoder = new Encoder();
            try
            {
                var content = encoder.DeserializeResponse(responseContent, typeof(String));
                Assert.True(false, "Should throw IOException with missing content type header");
            }
            catch (System.IO.IOException ex)
            {
                Assert.Equal("HTTP response did not have content-type header set", ex.Message);
            }
        }

        [Fact]
        public void DeserializeResponse_withJsonContentType()
        {
            var responseContent = new StringContent("{\"name\":\"braintree\"}", Encoding.UTF8, "application/json");

            var encoder = new Encoder();
            var content = encoder.DeserializeResponse(responseContent, typeof(TestData));

            Assert.NotNull(content);
            Assert.Equal("braintree", ((TestData)content).Name);
        }

        [Fact]
        public void DeserializeResponse_withTextContentType()
        {
            var responseContent = new StringContent("some plain text", Encoding.UTF8, "text/plain");

            var encoder = new Encoder();
            var content = encoder.DeserializeResponse(responseContent, typeof(String));

            Assert.NotNull(content);
            Assert.Equal("some plain text", content);
        }

        [Fact]
        public void DeserializeResponse_throwsForMultipartContentType()
        {
            var responseContent = new StringContent("some data", Encoding.UTF8, "multipart/form-data");

            var encoder = new Encoder();
            try
            {
                var content = encoder.DeserializeResponse(responseContent, typeof(String));
                Assert.True(false, "We do not deserialize multipart data");
            }
            catch (System.IO.IOException ex)
            {
                Assert.Equal("Unable to deserialize Content-Type: multipart/form-data.", ex.Message);
            }
        }

        [Fact]
        public void DeserializeResponse_throwsForFormEncodedContentType()
        {
            var responseContent = new StringContent("hello=world", Encoding.UTF8, "application/x-www-form-urlencoded");

            var encoder = new Encoder();
            try
            {
                var content = encoder.DeserializeResponse(responseContent, typeof(String));
                Assert.True(false, "form encoded deserialization not supported");
            }
            catch (System.IO.IOException ex)
            {
                Assert.Equal("Unable to deserialize Content-Type: application/x-www-form-urlencoded.", ex.Message);
            }
        }

        [Fact]
        public void DeserializeResponse_withGzipEncoding()
        {
            var encoder = new Encoder();

            var content = "hello world";
            var responseContent = new ByteArrayContent(Gzip(content));

            responseContent.Headers.Add("Content-Type", "text/plain");
            responseContent.Headers.ContentEncoding.Add("gzip");

            var deserializedContent = encoder.DeserializeResponse(responseContent, typeof(string));

            Assert.Equal(content, deserializedContent);
        }

        private static byte[] Gzip(string source)
        {
            var bytes = Encoding.UTF8.GetBytes(source);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    msi.CopyTo(gs);
                }

                return mso.ToArray();
            }
        }
    }
}
