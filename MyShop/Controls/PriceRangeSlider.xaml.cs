using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace MyShop.Controls;

public sealed partial class PriceRangeSlider : UserControl
{
    private enum DragTarget { None, Lower, Upper }

    private DragTarget _drag = DragTarget.None;
    private uint _pointerId = uint.MaxValue;

    public PriceRangeSlider()
    {
        InitializeComponent();
        LayoutRoot.SizeChanged += (_, _) => SyncVisuals();
        Loaded += (_, _) => SyncVisuals();
    }

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(PriceRangeSlider),
            new PropertyMetadata(0d, (d, _) => ((PriceRangeSlider)d).SyncVisuals()));

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(PriceRangeSlider),
            new PropertyMetadata(500d, (d, _) => ((PriceRangeSlider)d).SyncVisuals()));

    public double LowerValue
    {
        get => (double)GetValue(LowerValueProperty);
        set => SetValue(LowerValueProperty, value);
    }

    public static readonly DependencyProperty LowerValueProperty =
        DependencyProperty.Register(nameof(LowerValue), typeof(double), typeof(PriceRangeSlider),
            new PropertyMetadata(0d, (d, _) => ((PriceRangeSlider)d).SyncVisuals()));

    public double UpperValue
    {
        get => (double)GetValue(UpperValueProperty);
        set => SetValue(UpperValueProperty, value);
    }

    public static readonly DependencyProperty UpperValueProperty =
        DependencyProperty.Register(nameof(UpperValue), typeof(double), typeof(PriceRangeSlider),
            new PropertyMetadata(500d, (d, _) => ((PriceRangeSlider)d).SyncVisuals()));

    private void SyncVisuals()
    {
        var w = LayoutRoot.ActualWidth;
        if (w <= 0 || Maximum <= Minimum)
            return;

        var inner = Math.Max(0, w - 28);
        var span = Maximum - Minimum;
        var lo = (LowerValue - Minimum) / span;
        var hi = (UpperValue - Minimum) / span;

        ActiveTrack.Margin = new Thickness(14 + lo * inner, 0, 0, 0);
        ActiveTrack.Width = Math.Max(0, (hi - lo) * inner);
        ThumbLower.Margin = new Thickness(14 + lo * inner - 11, 0, 0, 0);
        ThumbUpper.Margin = new Thickness(14 + hi * inner - 11, 0, 0, 0);
    }

    private double ValueFromX(double x)
    {
        var w = LayoutRoot.ActualWidth;
        if (w <= 0 || Maximum <= Minimum)
            return Minimum;
        var inner = Math.Max(1e-6, w - 28);
        var ratio = (x - 14) / inner;
        ratio = Math.Clamp(ratio, 0, 1);
        return Math.Round(Minimum + ratio * (Maximum - Minimum));
    }

    private void ThumbLower_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        _drag = DragTarget.Lower;
        _pointerId = e.Pointer.PointerId;
        _ = LayoutRoot.CapturePointer(e.Pointer);
        e.Handled = true;
    }

    private void ThumbUpper_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        _drag = DragTarget.Upper;
        _pointerId = e.Pointer.PointerId;
        _ = LayoutRoot.CapturePointer(e.Pointer);
        e.Handled = true;
    }

    private void LayoutRoot_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (_drag == DragTarget.None)
            return;
        if (e.Pointer.PointerId != _pointerId)
            return;

        var x = e.GetCurrentPoint(LayoutRoot).Position.X;
        var v = ValueFromX(x);

        if (_drag == DragTarget.Lower)
        {
            var nv = Math.Min(v, UpperValue);
            nv = Math.Clamp(nv, Minimum, Maximum);
            if (Math.Abs(nv - LowerValue) > 1e-9)
                LowerValue = nv;
        }
        else
        {
            var nv = Math.Max(v, LowerValue);
            nv = Math.Clamp(nv, Minimum, Maximum);
            if (Math.Abs(nv - UpperValue) > 1e-9)
                UpperValue = nv;
        }

        e.Handled = true;
    }

    private void LayoutRoot_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (e.Pointer.PointerId != _pointerId)
            return;
        EndDrag(sender, e.Pointer);
        e.Handled = true;
    }

    private void LayoutRoot_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
    {
        if (e.Pointer.PointerId == _pointerId)
            EndDrag(sender, e.Pointer);
    }

    private void EndDrag(object sender, Pointer p)
    {
        if (_drag == DragTarget.None)
            return;
        _drag = DragTarget.None;
        _pointerId = uint.MaxValue;
        try
        {
            LayoutRoot.ReleasePointerCapture(p);
        }
        catch
        {
            // ignore
        }
    }
}
