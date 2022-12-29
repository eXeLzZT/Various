using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Various.Wpf.Controls;

internal class TextEditorWrapper
{
    private static readonly Type? TextEditorType = typeof(FrameworkElement).Assembly.GetType("System.Windows.Documents.TextEditor");
    private static readonly PropertyInfo? IsReadOnlyProp = TextEditorType?.GetProperty("IsReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly PropertyInfo? TextViewProp = TextEditorType?.GetProperty("TextView", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly MethodInfo? RegisterMethod = TextEditorType?.GetMethod("RegisterCommandHandlers", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(Type), typeof(bool), typeof(bool), typeof(bool) }, null);

    private static readonly Type? TextContainerType = typeof(FrameworkElement).Assembly.GetType("System.Windows.Documents.ITextContainer");
    private static readonly PropertyInfo? TextContainerTextViewProp = TextContainerType?.GetProperty("TextView");

    private static readonly PropertyInfo? TextContainerProp = typeof(TextBlock).GetProperty("TextContainer", BindingFlags.Instance | BindingFlags.NonPublic);

    private readonly object? _textEditor;

    internal TextEditorWrapper(object? textContainer, FrameworkElement uiScope, bool isUndoEnabled)
    {
        if (textContainer is null)
            throw new ArgumentException(nameof(textContainer));

        if (TextEditorType is null)
            throw new ArgumentException(nameof(TextEditorType));

        _textEditor = Activator.CreateInstance(TextEditorType, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.CreateInstance, null, new[] { textContainer, uiScope, isUndoEnabled }, null);
    }

    internal static void RegisterCommandHandlers(Type controlType, bool acceptsRichContent, bool readOnly, bool registerEventListeners)
    {
        RegisterMethod?.Invoke(null, new object[] { controlType, acceptsRichContent, readOnly, registerEventListeners });
    }

    internal static TextEditorWrapper CreateFor(TextBlock textBlock)
    {
        var textContainer = TextContainerProp?.GetValue(textBlock);
        var textEditor = new TextEditorWrapper(textContainer, textBlock, false);

        IsReadOnlyProp?.SetValue(textEditor._textEditor, true);
        TextViewProp?.SetValue(textEditor._textEditor, TextContainerTextViewProp?.GetValue(textContainer));

        return textEditor;
    }
}
