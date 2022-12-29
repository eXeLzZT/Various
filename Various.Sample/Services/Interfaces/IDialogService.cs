using System;

namespace Various.Sample.Services.Interfaces;

internal interface IDialogService
{
    void ShowDialog<TViewModel>(Action<bool?> callback);
}
