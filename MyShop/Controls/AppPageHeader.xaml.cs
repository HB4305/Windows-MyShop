using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace MyShop.Controls;

public sealed partial class AppPageHeader : UserControl
{
    public AppPageHeader()
    {
        InitializeComponent();
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(AppPageHeader), new PropertyMetadata(string.Empty));

    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public static readonly DependencyProperty SubtitleProperty =
        DependencyProperty.Register(nameof(Subtitle), typeof(string), typeof(AppPageHeader), new PropertyMetadata(string.Empty));

    public double TitleFontSize
    {
        get => (double)GetValue(TitleFontSizeProperty);
        set => SetValue(TitleFontSizeProperty, value);
    }

    public static readonly DependencyProperty TitleFontSizeProperty =
        DependencyProperty.Register(nameof(TitleFontSize), typeof(double), typeof(AppPageHeader), new PropertyMetadata(34d));

    public double SubtitleFontSize
    {
        get => (double)GetValue(SubtitleFontSizeProperty);
        set => SetValue(SubtitleFontSizeProperty, value);
    }

    public static readonly DependencyProperty SubtitleFontSizeProperty =
        DependencyProperty.Register(nameof(SubtitleFontSize), typeof(double), typeof(AppPageHeader), new PropertyMetadata(14d));

    public FontFamily TitleFontFamily
    {
        get => (FontFamily)GetValue(TitleFontFamilyProperty);
        set => SetValue(TitleFontFamilyProperty, value);
    }

    public static readonly DependencyProperty TitleFontFamilyProperty =
        DependencyProperty.Register(
            nameof(TitleFontFamily),
            typeof(FontFamily),
            typeof(AppPageHeader),
            new PropertyMetadata(new FontFamily("ms-appx:///Assets/Momo_Signature/MomoSignature-Regular.ttf#MomoSignature-Regular")));

    public FontFamily SubtitleFontFamily
    {
        get => (FontFamily)GetValue(SubtitleFontFamilyProperty);
        set => SetValue(SubtitleFontFamilyProperty, value);
    }

    public static readonly DependencyProperty SubtitleFontFamilyProperty =
        DependencyProperty.Register(
            nameof(SubtitleFontFamily),
            typeof(FontFamily),
            typeof(AppPageHeader),
            new PropertyMetadata(new FontFamily("ms-appx:///Assets/Fonts/MomoTrustSans-VariableFont_wght.ttf#Momo Trust Sans")));

    public Visibility SubtitleVisibility
    {
        get => (Visibility)GetValue(SubtitleVisibilityProperty);
        set => SetValue(SubtitleVisibilityProperty, value);
    }

    public static readonly DependencyProperty SubtitleVisibilityProperty =
        DependencyProperty.Register(nameof(SubtitleVisibility), typeof(Visibility), typeof(AppPageHeader), new PropertyMetadata(Visibility.Visible));
}
