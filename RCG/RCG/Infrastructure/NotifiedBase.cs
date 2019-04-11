using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RCG.Infrastructure
{
    abstract public class NotifiedBase : INotifyPropertyChanged
    {
        protected void Notify([CallerMemberName]string propName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public event PropertyChangedEventHandler PropertyChanged;
    }
}