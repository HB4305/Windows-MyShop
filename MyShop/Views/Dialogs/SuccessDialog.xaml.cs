namespace MyShop.Views.Dialogs;

public sealed partial class SuccessDialog : ContentDialog
{
	public SuccessDialog(string title, string content)
	{
		DialogTitle = title;
		DialogContent = content;
		InitializeComponent();
		Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
	}

	public string DialogTitle { get; }
	public string DialogContent { get; }

	private void CloseBtn_Click(object sender, RoutedEventArgs e)
	{
		Hide();
	}

	private ContentDialogResult _result = ContentDialogResult.None;

	public new async Task<ContentDialogResult> ShowAsync()
	{
		await base.ShowAsync();
		return _result;
	}
}
