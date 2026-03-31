namespace MyShop.Views.Dialogs;

public sealed partial class ConfirmationDialog : ContentDialog
{
	public ConfirmationDialog(string title, string content)
	{
		DialogTitle = title;
		DialogContent = content;
		InitializeComponent();
		Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
	}

	public string DialogTitle { get; }
	public string DialogContent { get; }
}
