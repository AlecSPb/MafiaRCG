using System;
using System.Windows.Input;

namespace RCG.Infrastructure
{
    public class RelayCommand : ICommand
    {
        Action<object> action;
        Predicate<object> predicate;

        public RelayCommand(Action<object> act, Predicate<object> pred = null)
        {
            action = act;
            predicate = pred;
        }

        public event EventHandler CanExecuteChanged;

        public void RaiseCanExecuteChanged() => CanExecuteChanged(this, null);

        public bool CanExecute(object parameter) => predicate != null ? predicate(parameter) : true;

        public void Execute(object parameter) => action(parameter);
    }
}
