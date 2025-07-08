namespace Various.Wpf.Controls;

internal readonly record struct ItemRange(int StartIndex, int EndIndex)
{
    public bool Contains(int itemIndex)
    {
        return itemIndex >= StartIndex && itemIndex <= EndIndex;
    }
}