using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace PayPalHttp
{
    public class JsonPartContent : StringContent
    {
        public JsonPartContent(string partName, object json):
            base(SerializeJson(json), System.Text.Encoding.UTF8, "application/json") {
            Headers.ContentType = new MediaTypeHeaderValue("application/json");
            // Need to ensure values are quoted
            Headers.Add("Content-Disposition", "form-data; name=\"" + partName + "\"; filename=\"" + partName + ".json\"");
        }

        private static string SerializeJson(object json) {
            var jsonSerializer = new DataContractJsonSerializer(json.GetType());

            using var ms = new MemoryStream();
            jsonSerializer.WriteObject(ms, json);
            ms.Position = 0;
            using var sr = new StreamReader(ms);
            return sr.ReadToEnd();
        }
    }
}
