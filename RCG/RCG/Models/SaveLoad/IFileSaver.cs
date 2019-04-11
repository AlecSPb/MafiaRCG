namespace RCG.Models.SaveLoad
{
    public interface IFileSaver<T>
    {
        void Save(T obj, string fileName = "Data.bin");
    }
}
