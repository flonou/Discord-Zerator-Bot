using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Main
{
    public class JsonClass<T> where T : new()
    {
        // Use a cache mechanisme to prevent files from being opened more than once
        private static Dictionary<string, JsonClass<T>> cache = new Dictionary<string, JsonClass<T>>();

        public T Data { get; private set; }

        public string FileName
        {
            get;
            private set;
        }

        public static JsonClass<T> Load(string fileName)
        {
            string fullFileName = Path.GetFullPath(fileName);
            if (cache.ContainsKey(fullFileName))
                return cache[fullFileName];

            if (!File.Exists(fullFileName))
            {
                var jsonClass = new JsonClass<T>();
                jsonClass.FileName = fileName;
                jsonClass.Data = new T();
                jsonClass.Save();
                Console.WriteLine(fullFileName + " file was not found, a new one was generated.");
                cache.Add(fullFileName, jsonClass);
                jsonClass.PostCreateFile();
                return jsonClass;
            }
            else
            {
                var jsonClass = new JsonClass<T>();
                // first, let's load our configuration file
                var json = File.ReadAllText(fullFileName, new UTF8Encoding(false));
                // convert json to class
                jsonClass.Data = JsonConvert.DeserializeObject<T>(json);
                jsonClass.FileName = fileName;
                cache.Add(fullFileName, jsonClass);
                // return
                return jsonClass;
            }
        }

        public Task Save()
        {

            JsonSerializer serializer = new JsonSerializer();
            //write string to file
            using (StreamWriter file = File.CreateText(FileName))
            using (JsonWriter writer = new JsonTextWriter(file))
            {
                //serialize object directly into file stream
                serializer.Serialize(writer, this.Data);
            }
            return Task.CompletedTask;
        }

        public virtual void PostCreateFile()
        {

        }
    }
}
