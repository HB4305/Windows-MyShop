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

	private void CancelBtn_Click(object sender, RoutedEventArgs e)
	{
		Hide();
	}

	private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
	{
		// Store the result before hiding so the caller can check
		_result = ContentDialogResult.Primary;
		Hide();
	}

	private ContentDialogResult _result = ContentDialogResult.None;

	public new async Task<ContentDialogResult> ShowAsync()
	{
		await base.ShowAsync();
		return _result;
	}
}
