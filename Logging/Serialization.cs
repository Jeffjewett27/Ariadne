using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Ariadne.Logging
{
    public class JsonHandler
    {
        public static string Serialize(object obj)
        {
            string jsonString = JsonConvert.SerializeObject(obj, Formatting.Indented,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                    Converters = { 
                        new StringEnumConverter(),
                        new TruncateFloatConverter()
                    }
                }
            );
            return jsonString;
        }
    }
    public class TruncateFloatConverter : JsonConverter<float>
    {
        public override float ReadJson(JsonReader reader, Type objectType, float existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            // We don't need to implement this for serialization only
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, float value, JsonSerializer serializer)
        {
            writer.WriteValue(Mathf.Round(value * 1000f) / 1000f);
        }
    }


}
