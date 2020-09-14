using Newtonsoft.Json;
using System;

namespace Helios.Settings
{
    [Serializable]
    public class Field<T>
    {
        [JsonIgnore]
        public Action<T> OnValueChanged;

        [JsonProperty]
        private Type _type => typeof(Field<T>);

        [JsonIgnore]
        public Type InnerType => typeof(T);

        [JsonProperty]
        private string _value;

        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.Indented
        };

        public void Set(T value)
        {
            string serializedValue = JsonConvert.SerializeObject(value, _serializerSettings);

            _value = serializedValue;
            OnValueChanged?.Invoke(value);
        }

        public T Get() => JsonConvert.DeserializeObject<T>(_value);
    }
}