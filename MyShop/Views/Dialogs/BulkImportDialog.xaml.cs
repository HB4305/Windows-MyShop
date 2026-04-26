using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MyShop.Views.Dialogs;

public sealed partial class BulkImportDialog : ContentDialog
{
    public string Message { get; set; } = string.Empty;
    public ContentDialogResult Result { get; private set; } = ContentDialogResult.None;

    public BulkImportDialog()
    {
        this.InitializeComponent();
        this.Loaded += OnDialogLoaded;
    }

    private void OnDialogLoaded(object sender, RoutedEventArgs e)
    {
        if (MessageLabel != null)
        {
            MessageLabel.Text = Message;
        }
    }

    private void OnImportClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        Result = ContentDialogResult.Primary;
    }
}
