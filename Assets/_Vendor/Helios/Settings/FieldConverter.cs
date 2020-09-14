using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Helios.Settings
{
    public class FieldConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject item = JObject.Load(reader);
            Type type;
            try
            {
                type = Type.GetType(item.GetValue("_type").ToString());
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Can't load type: {e.Message}");
                throw;
            }
            return item.ToObject(Type.GetType(item.GetValue("_type").ToString()));
        }

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }
    }
}
