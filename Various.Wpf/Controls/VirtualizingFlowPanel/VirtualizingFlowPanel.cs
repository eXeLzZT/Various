using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Various.Wpf.Controls;

public class VirtualizingFlowPanel : VirtualizingPanel, IScrollInfo
{
    #region DependencyProperty Orientation

    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(VirtualizingFlowPanel),
            new FrameworkPropertyMetadata(
                Orientation.Horizontal,
                FrameworkPropertyMetadataOptions.AffectsMeasure,
                OnOrientationChanged));

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var flowPanel = (VirtualizingFlowPanel)d;
        var orientation = (Orientation)e.NewValue;

        flowPanel?.SetMouseWheelScrollDirection(orientation);
    }

    #endregion

    #region DependencyProperty ItemHeight

    public static readonly DependencyProperty ItemHeightProperty =
        DependencyProperty.Register(
            nameof(ItemHeight),
            typeof(double),
            typeof(VirtualizingFlowPanel),
            new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public double ItemHeight
    {
        get => (double)GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    #endregion

    #region DependencyProperty MinItemWidth

    public static readonly DependencyProperty MinItemWidthProperty =
        DependencyProperty.Register(
            nameof(MinItemWidth),
            typeof(double),
            typeof(VirtualizingFlowPanel),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public double MinItemWidth
    {
        get => (double)GetValue(MinItemWidthProperty);
        set => SetValue(MinItemWidthProperty, value);
    }

    #endregion

    #region DependencyProperty MaxItemWidth

    public static readonly DependencyProperty MaxItemWidthProperty =
        DependencyProperty.Register(
            nameof(MaxItemWidth),
            typeof(double),
            typeof(VirtualizingFlowPanel),
            new FrameworkPropertyMetadata(double.PositiveInfinity, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public double MaxItemWidth
    {
        get => (double)GetValue(MaxItemWidthProperty);
        set => SetValue(MaxItemWidthProperty, value);
    }

    #endregion

    private const double ScrollLineDelta = 16.0;
    private const double MouseWheelDelta = 48.0;
    private const int ScrollLineDeltaItem = 1;
    private const int MouseWheelDeltaItem = 3;

    private Size _extent;
    private Size _viewport;
    private Point _offset;

    private DependencyObject? _itemsOwner;
    private ItemContainerGenerator? _itemContainerGenerator;
    private Visibility _previousVerticalScrollBarVisibility;
    private Visibility _previousHorizontalScrollBarVisibility;
    private Size _childSize;
    private ItemRange _itemRange;
    private int _itemsPerRowCount;
    private int _rowCount;
    private bool _itemsChanged;

    private ScrollDirection _mouseWheelScrollDirection = ScrollDirection.Horizontal;

    private new IRecyclingItemContainerGenerator ItemContainerGenerator
    {
        get
        {
            if (_itemContainerGenerator is null)
            {
                _ = InternalChildren;

                _itemContainerGenerator = base.ItemContainerGenerator.GetItemContainerGeneratorForPanel(this);
                _itemContainerGenerator.ItemsChanged += OnItemContainerGeneratorItemsChanged;
            }

            return _itemContainerGenerator;
        }
    }

    private DependencyObject ItemsOwner
    {
        get
        {
            if (_itemsOwner is null)
            {
                /* Use reflection to access internal method because the public
                 * GetItemsOwner method does always return the items control instead
                 * of the real items owner for example the group item when grouping */
                var getItemsOwnerInternalMethod = typeof(ItemsControl).GetMethod(
                    "GetItemsOwnerInternal",
                    BindingFlags.Static | BindingFlags.NonPublic,
                    null,
                    [typeof(DependencyObject)],
                    null
                )!;
                _itemsOwner = (DependencyObject)getItemsOwnerInternalMethod.Invoke(null, [this])!;
            }

            return _itemsOwner;
        }
    }

    private ItemsControl ItemsControl => ItemsControl.GetItemsOwner(this);
    private ReadOnlyCollection<object> Items => ((ItemContainerGenerator)ItemContainerGenerator).Items;
    private VirtualizationMode VirtualizationMode => GetVirtualizationMode(ItemsControl);
    private VirtualizationCacheLength CacheLength => GetCacheLength(ItemsControl);
    private VirtualizationCacheLengthUnit CacheLengthUnit => GetCacheLengthUnit(ItemsControl);
    private ScrollUnit ScrollUnit => GetScrollUnit(ItemsControl);

    public ScrollViewer? ScrollOwner { get; set; }
    public bool CanHorizontallyScroll { get; set; }
    public bool CanVerticallyScroll { get; set; }

    public double ExtentHeight => _extent.Height;
    public double ExtentWidth => _extent.Width;
    public double ViewportHeight => _viewport.Height;
    public double ViewportWidth => _viewport.Width;
    public double HorizontalOffset => _offset.X;
    public double VerticalOffset => _offset.Y;

    protected override bool CanHierarchicallyScrollAndVirtualizeCore => true;

    public Rect MakeVisible(Visual visual, Rect rectangle)
    {
        var position = visual.TransformToAncestor(this).Transform(_offset);
        var scrollAmountX = 0d;
        var scrollAmountY = 0d;

        if (position.X < _offset.X)
        {
            scrollAmountX = -(_offset.X - position.X);
        }
        else if (position.X + rectangle.Width > _offset.X + _viewport.Width)
        {
            var notVisibleX = position.X + rectangle.Width - (_offset.X + _viewport.Width);
            var maxScrollX = position.X - _offset.X;
            scrollAmountX = Math.Min(notVisibleX, maxScrollX);
        }

        if (position.Y < _offset.Y)
        {
            scrollAmountY = -(_offset.Y - position.Y);
        }
        else if (position.Y + rectangle.Height > _offset.Y + _viewport.Height)
        {
            var notVisibleY = position.Y + rectangle.Height - (_offset.Y + _viewport.Height);
            var maxScrollY = position.Y - _offset.Y;
            scrollAmountY = Math.Min(notVisibleY, maxScrollY);
        }

        SetHorizontalOffset(_offset.X + scrollAmountX);
        SetVerticalOffset(_offset.Y + scrollAmountY);

        var visibleRectWidth = Math.Min(rectangle.Width, _viewport.Width);
        var visibleRectHeight = Math.Min(rectangle.Height, _viewport.Height);

        return new Rect(scrollAmountX, scrollAmountY, visibleRectWidth, visibleRectHeight);
    }

    public void SetHorizontalOffset(double offset)
    {
        if (offset < 0 || _viewport.Width >= _extent.Width)
        {
            offset = 0;
        }
        else if (offset + _viewport.Width >= _extent.Width)
        {
            offset = _extent.Width - _viewport.Width;
        }

        _offset = _offset with { X = offset };

        ScrollOwner?.InvalidateScrollInfo();
        InvalidateMeasure();
    }

    public void SetVerticalOffset(double offset)
    {
        if (offset < 0 || _viewport.Height >= _extent.Height)
        {
            offset = 0;
        }
        else if (offset + _viewport.Height >= _extent.Height)
        {
            offset = _extent.Height - _viewport.Height;
        }

        _offset = _offset with { Y = offset };

        ScrollOwner?.InvalidateScrollInfo();
        InvalidateMeasure();
    }

    public void LineDown()
    {
        ScrollVertical(ScrollUnit == ScrollUnit.Pixel ? ScrollLineDelta : GetLineDownUpScrollAmount());
    }

    public void LineLeft()
    {
        ScrollHorizontal(ScrollUnit == ScrollUnit.Pixel ? -ScrollLineDelta : -GetLineRightLeftScrollAmount());
    }

    public void LineRight()
    {
        ScrollHorizontal(ScrollUnit == ScrollUnit.Pixel ? ScrollLineDelta : GetLineRightLeftScrollAmount());
    }

    public void LineUp()
    {
        ScrollVertical(ScrollUnit == ScrollUnit.Pixel ? -ScrollLineDelta : -GetLineDownUpScrollAmount());
    }

    public void MouseWheelDown()
    {
        if (_mouseWheelScrollDirection is ScrollDirection.Vertical)
        {
            ScrollVertical(ScrollUnit is ScrollUnit.Pixel ? MouseWheelDelta : GetMouseWheelDownUpScrollAmount());
        }
        else
        {
            MouseWheelRight();
        }
    }

    public void MouseWheelLeft()
    {
        ScrollHorizontal(ScrollUnit is ScrollUnit.Pixel ? -MouseWheelDelta : -GetMouseWheelRightLeftScrollAmount());
    }

    public void MouseWheelRight()
    {
        ScrollHorizontal(ScrollUnit is ScrollUnit.Pixel ? MouseWheelDelta : GetMouseWheelRightLeftScrollAmount());
    }

    public void MouseWheelUp()
    {
        if (_mouseWheelScrollDirection is ScrollDirection.Vertical)
        {
            ScrollVertical(ScrollUnit is ScrollUnit.Pixel ? -MouseWheelDelta : -GetMouseWheelDownUpScrollAmount());
        }
        else
        {
            MouseWheelLeft();
        }
    }

    public void PageDown()
    {
        ScrollVertical(_viewport.Height);
    }

    public void PageLeft()
    {
        ScrollHorizontal(-_viewport.Width);
    }

    public void PageRight()
    {
        ScrollHorizontal(_viewport.Width);
    }

    public void PageUp()
    {
        ScrollVertical(-_viewport.Height);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (MustIgnoreMeasure())
        {
            return availableSize;
        }

        UpdateComputedValues(availableSize);

        var extent =
            CreateOrientedSize(
                GetOrientedWidth(_childSize) * _itemsPerRowCount, GetOrientedHeight(_childSize) * _rowCount);

        var desiredWidth = Math.Min(availableSize.Width, extent.Width);
        var desiredHeight = Math.Min(availableSize.Height, extent.Height);
        var desiredSize = new Size(desiredWidth, desiredHeight);

        UpdateScrollInfo(desiredSize, extent);

        _itemRange = UpdateItemRange();

        RealizeItems();
        VirtualizeItems();

        foreach (UIElement child in InternalChildren)
        {
            child.Measure(_childSize);
        }

        return desiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var offsetX = GetOrientedX(_offset);
        var offsetY = GetOrientedY(_offset);

        for (var childIndex = 0; childIndex < InternalChildren.Count; childIndex++)
        {
            var child = InternalChildren[childIndex];
            var itemIndex = GetItemIndexFromChildIndex(childIndex);
            var columnIndex = itemIndex % _itemsPerRowCount;
            var rowIndex = itemIndex / _itemsPerRowCount;
            var x = columnIndex * GetOrientedWidth(_childSize);
            var y = rowIndex * GetOrientedHeight(_childSize);

            child.Arrange(CreateOrientedRect(x - offsetX, y - offsetY, _childSize.Width, _childSize.Height));
        }

        return finalSize;
    }

    protected override void OnItemsChanged(object sender, ItemsChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Replace:
                RemoveInternalChildRange(e.Position.Index, e.ItemUICount);
                break;
            case NotifyCollectionChangedAction.Move:
                RemoveInternalChildRange(e.OldPosition.Index, e.ItemUICount);
                break;
        }
    }

    private double GetLineDownUpScrollAmount()
    {
        return Math.Min(_childSize.Height * ScrollLineDeltaItem, _viewport.Height);
    }

    private double GetLineRightLeftScrollAmount()
    {
        return Math.Min(_childSize.Width * ScrollLineDeltaItem, _viewport.Width);
    }

    private double GetMouseWheelDownUpScrollAmount()
    {
        return Math.Min(_childSize.Height * MouseWheelDeltaItem, _viewport.Height);
    }

    private double GetMouseWheelRightLeftScrollAmount()
    {
        return Math.Min(_childSize.Width * MouseWheelDeltaItem, _viewport.Width);
    }

    private void ScrollVertical(double amount)
    {
        SetVerticalOffset(_offset.Y + amount);
    }

    private void ScrollHorizontal(double amount)
    {
        SetHorizontalOffset(_offset.X + amount);
    }

    private bool MustIgnoreMeasure()
    {
        if (_itemsChanged)
        {
            _itemsChanged = false;
            return false;
        }
        
        var scrollOwner = ScrollOwner;
        if (scrollOwner is null)
        {
            return false;
        }

        var verticalScrollBarGotHidden =
            scrollOwner.VerticalScrollBarVisibility is ScrollBarVisibility.Auto
            && scrollOwner.ComputedVerticalScrollBarVisibility is not Visibility.Visible
            && scrollOwner.ComputedVerticalScrollBarVisibility != _previousVerticalScrollBarVisibility;
        var horizontalScrollBarGotHidden =
            scrollOwner.HorizontalScrollBarVisibility is ScrollBarVisibility.Auto
            && scrollOwner.ComputedHorizontalScrollBarVisibility is not Visibility.Visible
            && scrollOwner.ComputedHorizontalScrollBarVisibility != _previousHorizontalScrollBarVisibility;

        _previousVerticalScrollBarVisibility = scrollOwner.ComputedVerticalScrollBarVisibility;
        _previousHorizontalScrollBarVisibility = scrollOwner.ComputedHorizontalScrollBarVisibility;

        return !scrollOwner.IsMeasureValid ||
               !scrollOwner.IsMeasureValid && verticalScrollBarGotHidden || horizontalScrollBarGotHidden;
    }

    private void UpdateComputedValues(Size availableSize)
    {
        var children = Items;
        var childCount = children.Count;
        var columnCount = 1;

        if (childCount is 0)
            return;

        var availableWidth = availableSize.Width;
        var availableHeight = availableSize.Height;
        var minItemWidth = MinItemWidth;
        var maxItemWidth = MaxItemWidth;

        if (availableWidth >= minItemWidth && availableWidth <= maxItemWidth)
        {
            _childSize = new Size(availableWidth, ItemHeight);
        }
        else if (availableWidth > maxItemWidth)
        {
            var maxColumns = (int)Math.Max(1, availableWidth / minItemWidth);
            var minColumns = (int)Math.Max(1, availableWidth / maxItemWidth);
            int maxRows;

            if (ItemsOwner is IHierarchicalVirtualizationAndScrollInfo)
            {
                maxRows = (int)Math.Ceiling((double)childCount / maxColumns);
            }
            else
            {
                maxRows = (int)Math.Max(1, availableHeight / ItemHeight);
            }
            
            columnCount = maxColumns;

            for (var columnIndex = minColumns; columnIndex <= maxColumns; columnIndex++)
            {
                if (childCount > columnIndex * maxRows)
                    continue;

                columnCount = columnIndex;
                break;
            }

            columnCount = Math.Min(childCount, columnCount);

            var childWith = Math.Max(minItemWidth, Math.Min(maxItemWidth, availableWidth / columnCount));
            _childSize = new Size(childWith, ItemHeight);
        }
        else
        {
            _childSize = new Size(minItemWidth, ItemHeight);
        }

        double itemsPerRowCount;

        if (ItemsOwner is IHierarchicalVirtualizationAndScrollInfo || columnCount == 1)
        {
            itemsPerRowCount = columnCount;
            _mouseWheelScrollDirection = ScrollDirection.Vertical;
        }
        else
        {
            itemsPerRowCount = Math.Max(1, GetOrientedWidth(availableSize) / GetOrientedWidth(_childSize));
            SetMouseWheelScrollDirection(Orientation);
        }

        _itemsPerRowCount = (int)Math.Floor(itemsPerRowCount);
        _rowCount = (int)Math.Ceiling((double)childCount / _itemsPerRowCount);
    }

    private void UpdateScrollInfo(Size desiredSize, Size extent)
    {
        var invalidateScrollInfo = false;

        if (extent != _extent)
        {
            _extent = extent;
            invalidateScrollInfo = true;
        }

        if (desiredSize != _viewport)
        {
            _viewport = desiredSize;
            invalidateScrollInfo = true;
        }

        if (ViewportHeight != 0 && VerticalOffset != 0 && VerticalOffset + ViewportHeight + 1 >= ExtentHeight)
        {
            _offset = _offset with { Y = extent.Height - desiredSize.Height };
            invalidateScrollInfo = true;
        }

        if (ViewportWidth != 0 && HorizontalOffset != 0 && HorizontalOffset + ViewportWidth + 1 >= ExtentWidth)
        {
            _offset = _offset with { X = extent.Width - desiredSize.Width };
            invalidateScrollInfo = true;
        }

        if (invalidateScrollInfo)
        {
            ScrollOwner?.InvalidateScrollInfo();
        }
    }

    private ItemRange UpdateItemRange()
    {
        var viewportStartPosition = GetOrientedY(_offset);
        var viewportEndPosition = GetOrientedY(_offset) + GetOrientedHeight(_viewport);

        if (CacheLengthUnit == VirtualizationCacheLengthUnit.Pixel)
        {
            viewportStartPosition = Math.Max(viewportStartPosition - CacheLength.CacheBeforeViewport, 0);
            viewportEndPosition =
                Math.Min(viewportEndPosition + CacheLength.CacheAfterViewport, GetOrientedHeight(_extent));
        }

        var startRowIndex = GetRowIndex(viewportStartPosition);
        var startIndex = startRowIndex * _itemsPerRowCount;

        var endRowIndex = GetRowIndex(viewportEndPosition);
        var endIndex = Math.Min(endRowIndex * _itemsPerRowCount + (_itemsPerRowCount - 1), Items.Count - 1);

        if (CacheLengthUnit == VirtualizationCacheLengthUnit.Page)
        {
            var itemsPerPage = endIndex - startIndex + 1;
            startIndex = Math.Max(startIndex - (int)CacheLength.CacheBeforeViewport * itemsPerPage, 0);
            endIndex = Math.Min(endIndex + (int)CacheLength.CacheAfterViewport * itemsPerPage, Items.Count - 1);
        }
        else if (CacheLengthUnit == VirtualizationCacheLengthUnit.Item)
        {
            startIndex = Math.Max(startIndex - (int)CacheLength.CacheBeforeViewport, 0);
            endIndex = Math.Min(endIndex + (int)CacheLength.CacheAfterViewport, Items.Count - 1);
        }

        return new ItemRange(startIndex, endIndex);
    }

    private int GetRowIndex(double location)
    {
        var calculatedRowIndex = (int)Math.Floor(location / GetOrientedHeight(_childSize));
        var maxRowIndex = (int)Math.Ceiling(Items.Count / (double)_itemsPerRowCount);

        return Math.Max(Math.Min(calculatedRowIndex, maxRowIndex), 0);
    }

    private void RealizeItems()
    {
        var startPosition = ItemContainerGenerator.GeneratorPositionFromIndex(_itemRange.StartIndex);
        var childIndex = startPosition.Offset == 0 ? startPosition.Index : startPosition.Index + 1;

        using (ItemContainerGenerator.StartAt(startPosition, GeneratorDirection.Forward, true))
        {
            for (var index = _itemRange.StartIndex; index <= _itemRange.EndIndex; index++, childIndex++)
            {
                var child = (UIElement)ItemContainerGenerator.GenerateNext(out var isNewlyRealized);

                if (!isNewlyRealized && InternalChildren.Contains(child))
                    continue;

                if (childIndex >= InternalChildren.Count)
                {
                    AddInternalChild(child);
                }
                else
                {
                    InsertInternalChild(childIndex, child);
                }

                ItemContainerGenerator.PrepareItemContainer(child);
                child.Measure(_childSize);
            }
        }
    }

    private void VirtualizeItems()
    {
        for (var childIndex = InternalChildren.Count - 1; childIndex >= 0; childIndex--)
        {
            var generatorPosition = new GeneratorPosition(childIndex, 0);
            var itemIndex = ItemContainerGenerator.IndexFromGeneratorPosition(generatorPosition);

            if (itemIndex == -1 || _itemRange.Contains(itemIndex))
                continue;

            if (VirtualizationMode is VirtualizationMode.Recycling)
            {
                ItemContainerGenerator.Recycle(generatorPosition, 1);
            }
            else
            {
                ItemContainerGenerator.Remove(generatorPosition, 1);
            }

            RemoveInternalChildRange(childIndex, 1);
        }
    }

    private int GetItemIndexFromChildIndex(int childIndex)
    {
        var generatorPosition = new GeneratorPosition(childIndex, 0);
        return ItemContainerGenerator.IndexFromGeneratorPosition(generatorPosition);
    }

    private double GetOrientedX(Point point)
    {
        return Orientation is Orientation.Vertical || _mouseWheelScrollDirection is ScrollDirection.Vertical
            ? point.X
            : point.Y;
    }

    private double GetOrientedY(Point point)
    {
        return Orientation is Orientation.Vertical || _mouseWheelScrollDirection is ScrollDirection.Vertical
            ? point.Y
            : point.X;
    }

    private double GetOrientedWidth(Size size)
    {
        return Orientation is Orientation.Vertical || _mouseWheelScrollDirection is ScrollDirection.Vertical
            ? size.Width
            : size.Height;
    }

    private double GetOrientedHeight(Size size)
    {
        return Orientation is Orientation.Vertical || _mouseWheelScrollDirection is ScrollDirection.Vertical
            ? size.Height
            : size.Width;
    }

    private Size CreateOrientedSize(double width, double height)
    {
        return Orientation is Orientation.Vertical || _mouseWheelScrollDirection is ScrollDirection.Vertical
            ? new Size(width, height)
            : new Size(height, width);
    }

    private Rect CreateOrientedRect(double x, double y, double width, double height)
    {
        return Orientation is Orientation.Vertical || _mouseWheelScrollDirection is ScrollDirection.Vertical
            ? new Rect(x, y, width, height)
            : new Rect(y, x, width, height);
    }

    private void SetMouseWheelScrollDirection(Orientation orientation)
    {
        _mouseWheelScrollDirection =
            orientation is Orientation.Vertical ? ScrollDirection.Vertical : ScrollDirection.Horizontal;
    }

    private void OnItemContainerGeneratorItemsChanged(object sender, ItemsChangedEventArgs e)
    {
        _itemsChanged = true;
    }
}