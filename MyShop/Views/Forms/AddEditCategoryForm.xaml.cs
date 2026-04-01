using MyShop.Models;

namespace MyShop.Views.Forms;

public sealed partial class AddEditCategoryForm : ContentDialog
{
	public AddEditCategoryForm(Category? category = null)
	{
		Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
		InitializeComponent();


		if (category is null)
		{
			Title = "Add category";
			CategoryName = string.Empty;
			CategoryDescription = string.Empty;
		}
		else
		{
			Title = "Edit category";
			CategoryName = category.Name;
			CategoryDescription = category.Description ?? string.Empty;
		}

		UpdatePrimaryButtonState();
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

	private void UpdatePrimaryButtonState()
	{
		IsPrimaryButtonEnabled = !IsNameInvalid;
	}

	private void CategoryNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
	{
		Bindings.Update();
		UpdatePrimaryButtonState();
	}
}
