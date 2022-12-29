using ReactiveUI;
using System.Reactive.Disposables;

namespace Various.Sample.Views
{
    /// <summary>
    /// Interaction logic for NotificationView.xaml
    /// </summary>
    public partial class NotificationView
    {
        public NotificationView()
        {
            InitializeComponent();

            this.WhenActivated(disposable =>
            {
                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Text,
                    view => view.TextBlockText.Text)
                .DisposeWith(disposable);
            });
        }
    }
}
