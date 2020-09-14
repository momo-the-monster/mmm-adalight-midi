using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Helios.Settings
{
    public static class Serializer
    {
#if UNITY_EDITOR
        private static readonly string DefaultPath = Path.Combine(Application.streamingAssetsPath, "Settings");
#else
        private static readonly string DefaultPath = Application.persistentDataPath;
#endif
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        { PreserveReferencesHandling = PreserveReferencesHandling.None };

        public static T Load<T>(string filename) where T : class => Load<T>(filename, DefaultPath);
        public static T Load<T>(string filename, string path) where T : class
        {
            string json = (Application.platform == RuntimePlatform.Android) ?
                ReadJsonFromAndroidFile(filename) : 
                ReadJsonFromFile(filename, path);

            if (json == null) return null;

            T group = JsonConvert.DeserializeObject<T>(json, SerializerSettings);
            return group;
        }

        private static string ReadJsonFromFile(string filename, string path)
        {
            if (!File.Exists(GetPath(filename, path)))
            {
                // Check StreamingAssets if the file doesn't exist in persistentData
                if (path.CompareTo(Application.persistentDataPath) == 0)
                {
                    var streamingPath = GetStreamingPathFor(filename);
                    if (!File.Exists(path)) return null;
                    return File.ReadAllText(GetPath(filename, streamingPath));
                }
                return null;
            }
            else
            {
                return File.ReadAllText(GetPath(filename, path));
            }
        }

        private static string ReadJsonFromAndroidFile(string filename)
        {
            string path = GetStreamingPathFor(filename);
            using (UnityWebRequest www = UnityWebRequest.Get(path))
            {
                www.SendWebRequest();
                while (!www.isDone) { };

                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                    return null;
                }
                else
                {
                    return www.downloadHandler.text;
                }
            }
        }

        public static bool Save<T>(string filename, T obj) => Save(filename, obj, DefaultPath);
        public static bool Save<T>(string filename, T obj, string path)
        {
            Directory.CreateDirectory(path);
            File.WriteAllText(GetPath(filename, path), JsonConvert.SerializeObject(obj, SerializerSettings));
            return File.Exists(GetPath(filename, path));
        }

        public static bool Delete(string filename)
        {
            string filepath = GetPath(filename);
            if (!File.Exists(filepath)) return false;

            File.Delete(filepath);

            return !File.Exists(filepath);
        }

        public static bool Reset(string filename)
        {
            // Handle Android separately because StreamingAssets are compressed
            if (Application.platform == RuntimePlatform.Android)
                return ResetAndroid(filename);

            string sPath = GetStreamingPathFor(filename);
            string aPath = GetPath(filename, Application.persistentDataPath);

            try
            {
                File.Copy(sPath, aPath, true);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Could Not Reset: {e.Message}");
                return false;
            }
            return true;
        }

        private static bool ResetAndroid(string filename)
        {
            string sPath = GetStreamingPathFor(filename);
            string aPath = GetPath(filename, Application.persistentDataPath);

            using (UnityWebRequest www = UnityWebRequest.Get(sPath))
            {
                www.SendWebRequest();
                while (!www.isDone) { };

                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                    return false;
                }
                else
                {
                    var json = www.downloadHandler.text;
                    File.WriteAllText(aPath, json);
                    return true;
                }
            }
        }

        private static string GetStreamingPathFor(string filename)
        {
            return $"{Path.Combine(Application.streamingAssetsPath, "Settings", filename)}.json";
        }

        public static string GetPath(string filename) => GetPath(filename, DefaultPath);
        public static string GetPath(string filename, string path) => Path.Combine(path, $"{filename}.json");
    }
}
