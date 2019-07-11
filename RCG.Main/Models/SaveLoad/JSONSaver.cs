using Newtonsoft.Json;
using System.IO;

namespace RCG.Main.Models.SaveLoad
{
    public class JSONSaver<T> : IFileSaver<T>
    {
        public void Save(T obj, string fileName = "Data.bin")
        {
            File.WriteAllText(fileName, JsonConvert.SerializeObject(obj, Formatting.Indented));
        }
    }
}
