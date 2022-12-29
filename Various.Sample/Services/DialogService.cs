using System;
using Various.Sample.Services.Interfaces;

namespace Various.Sample.Services;

internal class DialogService : IDialogService
{
    public void ShowDialog<TViewModel>(Action<bool?> callback)
    {
        ShowDialog(typeof(TViewModel), callback);
    }

    private void ShowDialog(Type type, Action<bool?> callback)
    {
        var dialog = new DialogWindow();

        EventHandler? closeEventHandler = null;
        closeEventHandler = (s, e) =>
        {
            callback(dialog.DialogResult);
            dialog.Closed -= closeEventHandler;
        };
        dialog.Closed += closeEventHandler;

        dialog.Content = Activator.CreateInstance(type);

        dialog.ShowDialog();
    }
}
