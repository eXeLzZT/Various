using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System.Windows.Input;
using Various.Sample.Services.Interfaces;
using Various.Wpf.Controls;

namespace Various.Sample.ViewModels;

public class MainViewModel : ReactiveObject, IUseReactiveModal
{
    private readonly IDialogService? _dialogService;

    [Reactive] public ReactiveObject ModalContent { get; set; }
    [Reactive] public bool IsModalOpen { get; set; }

    public ICommand CommandCloseDialog { get; }
    public ICommand CommandOpenDialog { get; }

    internal MainViewModel(IDialogService? dialogService = null)
    {
        _dialogService = dialogService ?? Locator.Current.GetService<IDialogService>();

        ModalContent = new NotificationViewModel("Hello this is a text.");

        CommandCloseDialog = ReactiveCommand.Create(CloseDialog);
        CommandOpenDialog = ReactiveCommand.Create(OpenDialog);
    }

    private void CloseDialog()
    {
        IsModalOpen = false;
    }


    private void OpenDialog()
    {
        IsModalOpen = true;
    }
}