using System.Windows;
using Various.Wpf.Helper;

namespace Various.Wpf.Controls;

public class SelectableTextBlock : HyperlinkTextBlock
{
    private readonly TextEditorWrapper _textEditor;

    static SelectableTextBlock()
    {
        FocusableProperty.OverrideMetadata(typeof(SelectableTextBlock), new FrameworkPropertyMetadata(true));
        FocusVisualStyleProperty.OverrideMetadata(typeof(SelectableTextBlock), new FrameworkPropertyMetadata(null));

        TextEditorWrapper.RegisterCommandHandlers(typeof(SelectableTextBlock), true, true, true);
    }

    public SelectableTextBlock()
    {
        _textEditor = TextEditorWrapper.CreateFor(this);
    }
}
