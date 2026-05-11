using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace MyShop.Controls;

public sealed partial class ShimmerLoadingControl : UserControl
{
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(ShimmerLoadingControl), new PropertyMetadata(new CornerRadius(4)));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public static readonly DependencyProperty IsShimmeringProperty =
        DependencyProperty.Register(nameof(IsShimmering), typeof(bool), typeof(ShimmerLoadingControl), new PropertyMetadata(true, OnIsShimmeringChanged));

    public bool IsShimmering
    {
        get => (bool)GetValue(IsShimmeringProperty);
        set => SetValue(IsShimmeringProperty, value);
    }

    private Storyboard? _shimmerStoryboard;

    public ShimmerLoadingControl()
    {
        this.InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateAnimation();
    }

    private static void OnIsShimmeringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ShimmerLoadingControl skeleton)
        {
            skeleton.UpdateAnimation();
        }
    }

    private void UpdateAnimation()
    {
        if (!IsLoaded) return;

        if (IsShimmering)
        {
            StartAnimation();
        }
        else
        {
            StopAnimation();
        }
    }

    private void StartAnimation()
    {
        if (_shimmerStoryboard == null)
        {
            _shimmerStoryboard = new Storyboard();
            var animation = new DoubleAnimation
            {
                From = -800,
                To = 800,
                Duration = new Duration(TimeSpan.FromSeconds(1.5)),
                RepeatBehavior = RepeatBehavior.Forever
            };

            Storyboard.SetTarget(animation, ShimmerTransform);
            Storyboard.SetTargetProperty(animation, "X");
            _shimmerStoryboard.Children.Add(animation);
        }

        _shimmerStoryboard.Begin();
        ShimmerRect.Visibility = Visibility.Visible;
    }

    private void StopAnimation()
    {
        _shimmerStoryboard?.Stop();
        ShimmerRect.Visibility = Visibility.Collapsed;
    }
}
