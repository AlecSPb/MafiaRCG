using Newtonsoft.Json;
using System;
using System.IO;

namespace RCG.Models.SaveLoad
{
    public class JSONLoader<T> : IFileLoader<T> where T : new()
    {
        public T Load(string fileName = "Data.bin")
        {
            if (File.Exists(fileName))
                return JsonConvert.DeserializeObject<T>(File.ReadAllText(fileName));
            else
                return new T();
        }
    }
}