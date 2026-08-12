using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

namespace BF.Utility
{
    public class JsonHandle
    {
        static string path = Application.dataPath;

        public static void Save<T>(T obj, string directoryPath, string fileName)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(string.Format("{0}/{1}", path, directoryPath));
            if (!Directory.Exists(builder.ToString()))
            {
                Directory.CreateDirectory(builder.ToString());
            }
            builder.Append("/" + fileName + ".txt");
            if (!File.Exists(builder.ToString()))
            {
                File.Create(builder.ToString()).Dispose();
            }
#if UNITY_EDITOR
            Debug.Log("Save");
            Debug.Log(builder.ToString());
#endif
            BinaryFormatter name1 = new BinaryFormatter();
            FileStream file = File.Open(builder.ToString(), FileMode.Truncate);
            var json = JsonUtility.ToJson(obj);
            name1.Serialize(file, json);
            file.Close();
        }
        public static void Load<T>(T obj, string directoryPath, string fileName)
        {
            BinaryFormatter name1 = new BinaryFormatter();
            StringBuilder builder = new StringBuilder();
            builder.Append(path + "/" + directoryPath);
            if (Directory.Exists(builder.ToString()))
            {
                builder.Append("/" + fileName + ".txt");
                if (File.Exists(builder.ToString()))
                {
#if UNITY_EDITOR
                    Debug.Log("Load");
#endif
                    FileStream file = File.Open(builder.ToString(), FileMode.Open);
                    if (file.Length != 0)
                    {
                        JsonUtility.FromJsonOverwrite((string)name1.Deserialize(file), obj);
                    }
                    file.Close();
                    return;
                }
            }

            Save(obj, directoryPath, fileName);
            Load(obj, directoryPath, fileName);
#if UNITY_EDITOR
            Debug.Log("Load Fail:No File, Auto create File");
#endif
        }
    }
}

