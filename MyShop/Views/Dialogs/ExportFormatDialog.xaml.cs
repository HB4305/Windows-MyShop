using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace MyShop.Views.Dialogs;

/// <summary>
/// Export format selected by the user in the format picker dialog.
/// </summary>
public enum ExportFormatMode
{
    None,
    Pdf,
    Xps
}

/// <summary>
/// Final refined version of ExportFormatDialog with manual button rendering
/// to ensure 100% brand color consistency in Skia.
/// </summary>
public sealed partial class ExportFormatDialog : ContentDialog
{
    private ExportFormatMode _selectedMode = ExportFormatMode.Pdf;
    private ContentDialogResult _customResult = ContentDialogResult.None;

    public ExportFormatMode SelectedMode => _selectedMode;
    public ContentDialogResult CustomResult => _customResult;

    public ExportFormatDialog(string orderNumber)
    {
        InitializeComponent();
        OrderIdText.Text = $"Order {orderNumber}";

        XpsOptionBorder.Visibility = Visibility.Visible;
        
        RefreshSelection();
    }

    private void RefreshSelection()
    {
        // Use the MyShop Purple brush defined in XAML resources
        var accentBrush = (Brush)Resources["AppAccentBrush"];
        var selectedBg = (Brush)Resources["SelectedCardBackgroundBrush"];
        var normalBg = (Brush)Application.Current.Resources["ControlFillColorDefaultBrush"];
        var normalBorder = (Brush)Application.Current.Resources["ControlStrokeColorDefaultBrush"];

        // PDF
        PdfOptionBorder.Background = _selectedMode == ExportFormatMode.Pdf ? selectedBg : normalBg;
        PdfOptionBorder.BorderBrush = _selectedMode == ExportFormatMode.Pdf ? accentBrush : normalBorder;
        PdfOptionBorder.BorderThickness = _selectedMode == ExportFormatMode.Pdf ? new Thickness(2) : new Thickness(1);
        PdfCheckMark.BorderBrush = _selectedMode == ExportFormatMode.Pdf ? accentBrush : normalBorder;
        PdfDot.Visibility = _selectedMode == ExportFormatMode.Pdf ? Visibility.Visible : Visibility.Collapsed;

        // XPS
        XpsOptionBorder.Background = _selectedMode == ExportFormatMode.Xps ? selectedBg : normalBg;
        XpsOptionBorder.BorderBrush = _selectedMode == ExportFormatMode.Xps ? accentBrush : normalBorder;
        XpsOptionBorder.BorderThickness = _selectedMode == ExportFormatMode.Xps ? new Thickness(2) : new Thickness(1);
        XpsCheckMark.BorderBrush = _selectedMode == ExportFormatMode.Xps ? accentBrush : normalBorder;
        XpsDot.Visibility = _selectedMode == ExportFormatMode.Xps ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        _customResult = ContentDialogResult.Primary;
        this.Hide();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        _customResult = ContentDialogResult.None;
        this.Hide();
    }

    private void PdfOptionBorder_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        _selectedMode = ExportFormatMode.Pdf;
        RefreshSelection();
    }

    private void XpsOptionBorder_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        _selectedMode = ExportFormatMode.Xps;
        RefreshSelection();
    }
}