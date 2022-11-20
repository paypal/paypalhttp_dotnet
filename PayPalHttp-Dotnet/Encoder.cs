using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PayPalHttp
{
    public class Encoder
    {
        private static readonly Dictionary<string, ISerializer> DefaultSerializers = new Dictionary<string, ISerializer>();

        private readonly Dictionary<string, ISerializer> _serializerLookup;

        static Encoder()
        {
            RegisterSerializer(new JsonSerializer(), DefaultSerializers);
            RegisterSerializer(new TextSerializer(), DefaultSerializers);
            RegisterSerializer(new MultipartSerializer(), DefaultSerializers);
            RegisterSerializer(new FormEncodedSerializer(), DefaultSerializers);
        }

        public Encoder()
        {
            _serializerLookup = new Dictionary<string, ISerializer>(DefaultSerializers);
        }

        private static void RegisterSerializer(ISerializer serializer, Dictionary<string, ISerializer> serializerLookup)
        {
            if (serializer != null)
            {
                serializerLookup[serializer.GetContentTypeRegexPattern()] = serializer;
            }
        }

        public void RegisterSerializer(ISerializer serializer)
        {
            RegisterSerializer(serializer, _serializerLookup);
        }

        public HttpContent SerializeRequest(HttpRequest request)
        {
            if (request.ContentType == null)
            {
                throw new IOException("HttpRequest did not have content-type header set");
            }

            request.ContentType = request.ContentType.ToLower();
            
            ISerializer serializer = GetSerializer(request.ContentType);
            if (serializer == null)
            {
                throw new IOException($"Unable to serialize request with Content-Type {request.ContentType}. Supported encodings are {GetSupportedContentTypes()}");
            }

            var content = serializer.Encode(request);

            if ("gzip".Equals(request.ContentEncoding))
            {
                var source = content.ReadAsStringAsync().Result;
                content = new ByteArrayContent(Gzip(source));
            }

            return content;
        }

        public object DeserializeResponse(HttpContent content, Type responseType)
        {
            if (content.Headers.ContentType == null)
            {
                throw new IOException("HTTP response did not have content-type header set");
            }
            var contentType = content.Headers.ContentType.ToString();
            contentType = contentType.ToLower();
            ISerializer serializer = GetSerializer(contentType);
            if (serializer == null)
            {
                throw new IOException($"Unable to deserialize response with Content-Type {contentType}. Supported encodings are {GetSupportedContentTypes()}");
            }

            var contentEncoding = content.Headers.ContentEncoding.FirstOrDefault();

            if ("gzip".Equals(contentEncoding))
            {
                var buf = content.ReadAsByteArrayAsync().Result;
                content = new StringContent(Gunzip(buf), Encoding.UTF8);
            }

            return serializer.Decode(content, responseType);
        }

        private ISerializer GetSerializer(string contentType)
        {
            return _serializerLookup.Values.FirstOrDefault(f => f.GetContentRegEx().Match(contentType).Success);
        }

        private string GetSupportedContentTypes()
        {
            return String.Join(", ", _serializerLookup.Keys);
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

        private static string Gunzip(byte[] source)
        {
            using (var msi = new MemoryStream(source))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }

        private static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }
    }
}
