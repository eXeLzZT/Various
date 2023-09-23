using System;
using System.ComponentModel;
using System.Windows;

namespace Various.Sample.Services.Interfaces;

internal interface IDialogService
{
    void ShowDialog<TViewModel>(
        Action<Window> activatedCallback, 
        Action<Window, CancelEventArgs> closingCallback, 
        Action<bool?> closedCallback);

    void Show<TViewModel>(
        Action<Window> activatedCallback, 
        Action<Window, CancelEventArgs> closingCallback);
}
