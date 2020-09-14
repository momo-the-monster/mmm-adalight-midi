using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Helios.Settings
{
    [Serializable]
    public class Group
    {
        public string Name { get; }
        [JsonIgnore]
        public string Path => Serializer.GetPath(Name);

        [JsonProperty(ItemConverterType = typeof(FieldConverter))]
        private readonly Dictionary<string, object> _fields = new Dictionary<string, object>();
        private static Dictionary<string, Group> _loadedGroups = new Dictionary<string, Group>();

        public Group(string name)
        {
            Name = name;

#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += change => {
                if (change == UnityEditor.PlayModeStateChange.ExitingPlayMode) Save();
            };
#else
            Application.quitting += () => { Save(); };
#endif
        }

        // return Group from disk or cache
        public static Group Get(string groupName)
        {
            // if group exists in memory, load it
            if (_loadedGroups.ContainsKey(groupName))
            {
                return _loadedGroups[groupName];
            }

            // if group exists on disk, load it
            Group group = Serializer.Load<Group>(groupName);
            if (group != null)
            {
                _loadedGroups.Add(groupName, group);
                return group;
            }

            // else create and add to _loadedGroups
            group = new Group(groupName);
            _loadedGroups.Add(groupName, group);

            return group;
        }

        public T Get<T>(string fieldName, T defaultValue = default)
        {
            if (!_fields.ContainsKey(fieldName))
            {
                Debug.LogWarning($"Called Get on non-existing field ({fieldName}) for type ({typeof(T)}) - Creating the Field.");
                Set(fieldName, defaultValue);
                return defaultValue;
            }
            return ((Field<T>)_fields[fieldName]).Get();
        }

        public Group Set<T>(string fieldName, T value)
        {
            if (!_fields.ContainsKey(fieldName)) _fields.Add(fieldName, new Field<T>());

            ((Field<T>)_fields[fieldName]).Set(value);
            return this;
        }

        public List<(string, object)> GetFields()
        {
            List<(string, object)> fields = new List<(string, object)>();
            foreach (KeyValuePair<string, dynamic> item in _fields)
            {
                fields.Add((item.Key, (object)item.Value));
            }
            return fields;
        }

        public bool Save() => Serializer.Save(Name, this);

        public bool Reset() {
            // remove from cache to force a reload
            _loadedGroups.Remove(Name);
            bool result = Serializer.Reset(Name);
            return result;
        }

        public void DeleteField(string fieldName)
        {
            if (!_fields.ContainsKey(fieldName))
            {
                Debug.LogWarning($"Tried to delete {fieldName} which does not exist in {Name}");
                return;
            }
            _fields.Remove(fieldName);
            Save();
        }

        public Group DeleteGroup()
        {
            Serializer.Delete(Name);
            return this;
        }

        public int GetFieldCount() => _fields.Count;

        public bool Subscribe<T>(string fieldName, Action<T> callback)
        {
            if (!_fields.ContainsKey(fieldName)) return false;

            ((Field<T>)_fields[fieldName]).OnValueChanged += callback;
            return true;
        }

        public bool Unsubscribe<T>(string fieldName, Action<T> callback)
        {
            if (!_fields.ContainsKey(fieldName)) return false;

            ((Field<T>)_fields[fieldName]).OnValueChanged -= callback;
            return true;
        }

    }

}