using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Helios.Settings
{
    [Serializable]
    public class SuperGroup
    {
        public string Name { get; }
        [Newtonsoft.Json.JsonIgnore]
        public string Path => Serializer.GetPath(Name);
        public ReadOnlyCollection<string> Groups => _groupNames.AsReadOnly();

        private List<string> _groupNames { get; }
        private static Dictionary<string, SuperGroup> _loadedSuperGroups = new Dictionary<string, SuperGroup>();

        public SuperGroup(string name, List<string> groups = null)
        {
            Name = name;
            _groupNames = groups ?? new List<string>();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += change => {
                if (change == UnityEditor.PlayModeStateChange.ExitingPlayMode) Save();
            };
#else
            Application.quitting += () => { Save(); };
#endif
        }

        // Add group to list if it's not in there already
        public Group Set(string groupName)
        {
            if (!_groupNames.Contains(groupName)) _groupNames.Add(groupName);
            return Group.Get(groupName);
        }

        public List<Group> GetGroups()
        {
            List<Group> groups = new List<Group>();
            foreach (string group in _groupNames)
            {
                groups.Add(Group.Get(group));
            }
            return groups;
        }

        public int GetGroupCount()
        {
            return _groupNames.Count;
        }

        // return SuperGroup from disk or cache
        public static SuperGroup Get(string name)
        {
            // if group exists in memory, load it
            if (_loadedSuperGroups.ContainsKey(name))
            {
                return _loadedSuperGroups[name];
            }

            // if group exists on disk, load it
            SuperGroup superGroup = Serializer.Load<SuperGroup>(name);
            if (superGroup != null)
            {
                return superGroup;
            }

            // else create and add to _loadedSuperGroups
            superGroup = new SuperGroup(name);
            _loadedSuperGroups.Add(name, superGroup);

            return superGroup;
        }

        public void DeleteGroup(string groupName)
        {
            if (!_groupNames.Contains(groupName))
            {
                Debug.LogWarning($"Tried to delete {groupName} which does not exist in {Name}");
                return;
            }
            _groupNames.Remove(groupName);
            Save();
        }

        public bool Save() => Serializer.Save(Name, this);

        public SuperGroup Delete()
        {
            Serializer.Delete(Name);
            return this;
        }
    }
}