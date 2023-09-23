using System;
using System.ComponentModel;
using System.Windows;
using Various.Sample.Services.Interfaces;

namespace Various.Sample.Services;

internal class DialogService : IDialogService
{
    public void ShowDialog<TViewModel>(
        Action<Window> activatedCallback,
        Action<Window, CancelEventArgs> closingCallback,
        Action<bool?> closedCallback)
    {
        ShowDialog(typeof(TViewModel), activatedCallback, closingCallback, closedCallback);
    }

    public void Show<TViewModel>(Action<Window> activatedCallback, Action<Window, CancelEventArgs> closingCallback)
    {
        Show(typeof(TViewModel), activatedCallback, closingCallback);
    }

    private void ShowDialog(
        Type type,
        Action<Window> activatedCallback, 
        Action<Window, CancelEventArgs> closingCallback,
        Action<bool?> closedCallback)
    {
        var dialog = new DialogWindow();

        EventHandler? activatedEventHandler = null;
        activatedEventHandler = (s, e) =>
        {
            activatedCallback(dialog);
            dialog.Activated -= activatedEventHandler;
        };
        dialog.Activated += activatedEventHandler;
        
        CancelEventHandler? closingEventHandler = null;
        closingEventHandler = (s, e) =>
        {
            closingCallback(dialog, e);
            dialog.Closing -= closingEventHandler;
        };
        dialog.Closing += closingEventHandler;

        EventHandler? closeEventHandler = null;
        closeEventHandler = (s, e) =>
        {
            closedCallback(dialog.DialogResult);
            dialog.Closed -= closeEventHandler;
        };
        dialog.Closed += closeEventHandler;

        dialog.Content = Activator.CreateInstance(type);

        dialog.ShowDialog();
    }

    private void Show(
        Type type,
        Action<Window> activatedCallback, 
        Action<Window, CancelEventArgs> closingCallback)
    {
        var window = new DialogWindow();
        
        EventHandler? activatedEventHandler = null;
        activatedEventHandler = (s, e) =>
        {
            activatedCallback(window);
            window.Activated -= activatedEventHandler;
        };
        window.Activated += activatedEventHandler;
        
        CancelEventHandler? closingEventHandler = null;
        closingEventHandler = (s, e) =>
        {
            closingCallback(window, e);
            window.Closing -= closingEventHandler;
        };
        window.Closing += closingEventHandler;
        
        window.Content = Activator.CreateInstance(type);

        window.Show();
    }
}