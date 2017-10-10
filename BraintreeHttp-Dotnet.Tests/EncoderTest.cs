using System;
using System.Net.Http;
using Xunit;
using System.Text;
using System.Collections.Generic;
using System.IO;

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
        public async void SerializeRequest_WithJsonContentTypeAsync()
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
        public void SerializeRequest_WithMultipartContentTypeAsync()
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
        public async void SerializeRequest_WithTextContentTypeAsync()
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
        public async void SerializeReqeust_WithFormEncodedContentType()
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
        public void DeserializeResponse_WithJsonContentType()
        {
            var responseContent = new StringContent("{\"name\":\"braintree\"}", Encoding.UTF8, "application/json");

            var encoder = new Encoder();
            var content = encoder.DeserializeResponse(responseContent, typeof(TestData));

            Assert.NotNull(content);
            Assert.Equal("braintree", ((TestData)content).Name);
        }

        [Fact]
        public void DeserializeResponse_WithTextContentType()
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
    }
}
