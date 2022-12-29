using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Various.Wpf.Controls;

[TemplatePart(Name = "PART_Content", Type = typeof(Border))]
public class ReactiveModal : ContentControl
{
    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(
            nameof(IsOpen),
            typeof(bool),
            typeof(ReactiveModal),
            new PropertyMetadata(false));

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    private Border? _content;

    static ReactiveModal()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ReactiveModal), new FrameworkPropertyMetadata(typeof(ReactiveModal)));
        BackgroundProperty.OverrideMetadata(typeof(ReactiveModal), new FrameworkPropertyMetadata(CreateDefaultBrush()));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _content = GetTemplateChild("PART_Content") as Border;
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);

        if (!_content?.IsMouseOver ?? false)
            IsOpen = false;
    }

    private static object CreateDefaultBrush()
    {
        return new SolidColorBrush(Colors.Black)
        {
            Opacity = 0.3
        };
    }
}
