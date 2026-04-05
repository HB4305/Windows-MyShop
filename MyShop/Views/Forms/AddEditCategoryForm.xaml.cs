using MyShop.Models;

namespace MyShop.Views.Forms;

public sealed partial class AddEditCategoryForm : ContentDialog
{
	private ContentDialogResult _result = ContentDialogResult.None;

	public AddEditCategoryForm(Category? category = null)
	{
		Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
		InitializeComponent();

		if (category is null)
		{
			FormTitle.Text = "Add Category";
			FormSubtitle.Text = "Create a new category for your product catalog.";
			SaveBtnText.Text = "Create Category";
			CategoryName = string.Empty;
			CategoryDescription = string.Empty;
		}
		else
		{
			FormTitle.Text = "Edit Category";
			FormSubtitle.Text = "Update the details of this category.";
			SaveBtnText.Text = "Save Changes";
			CategoryName = category.Name;
			CategoryDescription = category.Description ?? string.Empty;
		}

		UpdateSaveButtonState();
		Loaded += (_, _) => CategoryNameTextBox.Focus(FocusState.Programmatic);
	}

	public string CategoryName { get; set; } = string.Empty;

	public string CategoryDescription { get; set; } = string.Empty;

	public bool IsNameEmpty => string.IsNullOrWhiteSpace(CategoryName);

	public bool IsNameTooLong => CategoryName?.Length > 100;

	public bool IsNameInvalid => IsNameEmpty || IsNameTooLong;

	public string NameValidationMessage
	{
		get
		{
			if (IsNameEmpty) return "Category name is required";
			if (IsNameTooLong) return "Category name must not exceed 100 characters";
			return string.Empty;
		}
	}

	public string NormalizedName => CategoryName.Trim();

	public string? NormalizedDescription
	{
		get
		{
			var value = CategoryDescription.Trim();
			return string.IsNullOrWhiteSpace(value) ? null : value;
		}
	}

	private void UpdateSaveButtonState()
	{
		SaveBtn.IsEnabled = !IsNameInvalid;
	}

	private void CategoryNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
	{
		Bindings.Update();
		UpdateSaveButtonState();
	}

	private void CancelBtn_Click(object sender, RoutedEventArgs e)
	{
		Hide();
	}

	private void SaveBtn_Click(object sender, RoutedEventArgs e)
	{
		_result = ContentDialogResult.Primary;
		Hide();
	}

	public new async Task<ContentDialogResult> ShowAsync()
	{
		await base.ShowAsync();
		return _result;
	}
}
