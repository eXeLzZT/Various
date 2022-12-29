using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Various.Wpf;

public class RelayCommand<T> : ICommand
{
    private readonly Action<T> _execute;
    private readonly Predicate<T>? _canExecute;

    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public RelayCommand(Action<T?> execute, Predicate<T>? canExecute = null)
    {
        if (execute == null)
            throw new ArgumentNullException();

        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object parameter)
    {
        return _canExecute is null || _canExecute((T)parameter);
    }
    
    public void Execute(object parameter)
    {
        _execute((T)parameter);
    }
}
