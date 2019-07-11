namespace RCG.Main.Models.SaveLoad
{
    public interface IFileLoader<T> where T : new()
    {
        T Load(string fileName = "Data.bin");
    }
}
