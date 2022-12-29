using ReactiveUI;

namespace Various.Sample.ViewModels;

public class NotificationViewModel : ReactiveObject
{
    public string Text { get; }

    public NotificationViewModel(string text)
    {
        Text = text;
    }
}
