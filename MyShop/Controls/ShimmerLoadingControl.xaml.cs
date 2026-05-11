using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.Foundation;

namespace MyShop.Controls;

public sealed partial class ShimmerLoadingControl : UserControl
{
    // ── Dependency Properties ────────────────────────────────────────────────

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius),
            typeof(ShimmerLoadingControl), new PropertyMetadata(new CornerRadius(4)));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public static readonly DependencyProperty IsShimmeringProperty =
        DependencyProperty.Register(nameof(IsShimmering), typeof(bool),
            typeof(ShimmerLoadingControl), new PropertyMetadata(true, OnIsShimmeringChanged));

    public bool IsShimmering
    {
        get => (bool)GetValue(IsShimmeringProperty);
        set => SetValue(IsShimmeringProperty, value);
    }

    // ── Fields ───────────────────────────────────────────────────────────────

    private Storyboard? _storyboard;
    private DoubleAnimation? _animation;

    // ── Constructor ──────────────────────────────────────────────────────────

    public ShimmerLoadingControl() => InitializeComponent();

    // ── Lifecycle Handlers ───────────────────────────────────────────────────

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ApplyClipAndSize();
        if (IsShimmering) StartAnimation();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        ApplyClipAndSize();
        // Restart so From/To values reflect the new width
        if (IsShimmering)
        {
            DisposeAnimation();
            StartAnimation();
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e) => DisposeAnimation();

    // ── Animation ────────────────────────────────────────────────────────────

    private static void OnIsShimmeringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ShimmerLoadingControl ctrl && ctrl.IsLoaded)
        {
            if ((bool)e.NewValue) ctrl.StartAnimation();
            else ctrl.StopAnimation();
        }
    }

    /// <summary>
    /// Sets RootGrid.Clip and ShimmerRect.Width to match current ActualWidth/Height,
    /// preventing any overflow of the shimmer rectangle outside the control bounds.
    /// </summary>
    private void ApplyClipAndSize()
    {
        var w = ActualWidth;
        var h = ActualHeight;
        if (w <= 0 || h <= 0) return;

        // Hard clip — shimmer cannot bleed outside the rounded rectangle
        RootGrid.Clip = new RectangleGeometry { Rect = new Rect(0, 0, w, h) };

        // Size the shimmer to the control width
        // The gradient brush (transparent→shimmer→transparent) handles the fade
        ShimmerRect.Width = w;
    }

    private void StartAnimation()
    {
        var w = ActualWidth;
        if (w <= 0) return;

        _animation = new DoubleAnimation
        {
            From = -w,          // Start fully off-screen to the left (outside clip)
            To   =  w,          // End fully off-screen to the right (outside clip)
            Duration           = new Duration(TimeSpan.FromSeconds(1.5)),
            RepeatBehavior     = RepeatBehavior.Forever,
            EnableDependentAnimation = false   // GPU-composited where possible
        };

        Storyboard.SetTarget(_animation, ShimmerTransform);
        Storyboard.SetTargetProperty(_animation, "X");

        _storyboard = new Storyboard();
        _storyboard.Children.Add(_animation);
        _storyboard.Begin();

        ShimmerRect.Visibility = Visibility.Visible;
    }

    private void StopAnimation()
    {
        _storyboard?.Stop();
        if (ShimmerRect is not null)
            ShimmerRect.Visibility = Visibility.Collapsed;
    }

    private void DisposeAnimation()
    {
        _storyboard?.Stop();
        _storyboard = null;
        _animation  = null;
    }
}
