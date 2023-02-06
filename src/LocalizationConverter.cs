using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LocalizationLib
{
    public class LocalizationConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(LocalizationNode);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            JToken t = JToken.Load(reader);

            if(t.Type == JTokenType.Object)
            {
                return new LocalizationNode(JsonConvert.DeserializeObject<Dictionary<string, LocalizationNode>>(t.ToString(), new LocalizationConverter()));
            }
            if(t.Type == JTokenType.String)
            {
                return new LocalizationNode(t.ToString());
            }

            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if(value is null) throw new NullReferenceException();
            JToken t = JToken.FromObject(value);

            if(value is not LocalizationNode node)
            {
                t.WriteTo(writer);
                return;
            }

            object? obj = null;

            if(node.IsCategory)
            {
                if(node.Nodes is null) throw new Exception();
                obj = node.Nodes;
            }
            else if(node.IsString)
            {
                if(node.Value is null) throw new Exception();
                obj = node.Value;
            }

            serializer.Serialize(writer, obj);
        }
    }
}