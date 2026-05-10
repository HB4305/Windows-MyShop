namespace MyShop.Views.Dialogs;

public sealed partial class ConfirmationDialog : ContentDialog
{
	public ConfirmationDialog(string title, string content, string confirmText = "Confirm", string glyph = "\uE7BA")
	{
		DialogTitle = title;
		DialogContent = content;
		ConfirmButtonText = confirmText;
		IconGlyph = glyph;
		InitializeComponent();
		Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
	}

	public string DialogTitle { get; }
	public string DialogContent { get; }
	public string ConfirmButtonText { get; }
	public string IconGlyph { get; }

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
