using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Various.Utils;

namespace Various.Wpf.Controls;

public class HyperlinkTextBlock : TextBlock
{
    #region DependencyProperty Text

    public static new readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(HyperlinkTextBlock));

    private static readonly DependencyPropertyDescriptor TextPropertyDescriptor =
        DependencyPropertyDescriptor.FromProperty(TextProperty, typeof(HyperlinkTextBlock));

    public new string Text
    {
        get => (string)TextPropertyDescriptor.GetValue(this);
        set => TextPropertyDescriptor.SetValue(this, value);
    }

    #endregion

    static HyperlinkTextBlock()
    {
        TextProperty.OverrideMetadata(typeof(HyperlinkTextBlock),
            new FrameworkPropertyMetadata(
                TextProperty.GetMetadata(typeof(TextBlock)).DefaultValue,
                (o, e) => (o as HyperlinkTextBlock)?.OnTextChanged((string)e.NewValue)));
    }

    protected virtual void OnTextChanged(string text) => RecomputeInlines(text);

    private void RecomputeInlines(string text)
    {
        Inlines.Clear();

        if (string.IsNullOrEmpty(text))
            return;

        var textPos = 0;
        var urlMatch = text.Match(textPos);

        //TODO: Exception if the last inline is a hyperlink

        while (urlMatch.Success)
        {
            if (urlMatch.Index != textPos)
                Inlines.Add(text.Substring(textPos, urlMatch.Index - textPos));

            Inlines.Add(CreateHyperlink(urlMatch.Value));

            textPos = urlMatch.Index + urlMatch.Length;
            urlMatch = text.Match(textPos);
        }

        if (textPos != text.Length)
            Inlines.Add(text.Substring(textPos));
    }

    private Hyperlink CreateHyperlink(string text)
    {
        var navigateUri = new Uri(text);
        var hyperlink = new Hyperlink()
        {
            NavigateUri = navigateUri
        };
        hyperlink.Inlines.Add(text);

        hyperlink.RequestNavigate += (o, e) =>
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)
            {
                UseShellExecute = true
            });

            e.Handled = true;
        };

        return hyperlink;
    }
}
