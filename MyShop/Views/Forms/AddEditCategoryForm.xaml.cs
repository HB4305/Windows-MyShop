using MyShop.Models;
using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace MyShop.Views.Forms;

public sealed partial class AddEditCategoryForm : ContentDialog, INotifyPropertyChanged
{
	private ContentDialogResult _result = ContentDialogResult.None;
	public event PropertyChangedEventHandler? PropertyChanged;

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
			_isDirty = true; // For edit, we want to show validation if they clear it
		}

		// Don't call UpdateSaveButtonState here to avoid premature validation
		Loaded += (_, _) => CategoryNameTextBox.Focus(FocusState.Programmatic);
	}

	private bool _isDirty = false;

	public string CategoryName { get; set; } = string.Empty;

	public string CategoryDescription { get; set; } = string.Empty;

	public bool IsNameEmpty => string.IsNullOrWhiteSpace(CategoryName);

	public bool IsNameTooLong => CategoryName?.Length > 100;

	public bool IsNameInvalid => IsNameEmpty || IsNameTooLong;

	public string NameValidationMessage
	{
		get
		{
			if (!_isDirty) return string.Empty;
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

	private void SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
	{
		if (Equals(storage, value)) return;
		storage = value;
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	private void TextBox_GotFocus(object sender, RoutedEventArgs e)
	{
		if (sender is TextBox tb)
		{
			if (tb == CategoryNameTextBox) CategoryNameWrapper.BorderBrush = (Brush)ThemeResource.GetResource("AppPurpleBrush");
			else if (tb == DescriptionTextBox) DescriptionWrapper.BorderBrush = (Brush)ThemeResource.GetResource("AppPurpleBrush");
		}
	}

	private void TextBox_LostFocus(object sender, RoutedEventArgs e)
	{
		if (sender is TextBox tb)
		{
			if (tb == CategoryNameTextBox) CategoryNameWrapper.BorderBrush = (Brush)ThemeResource.GetResource("ControlStrokeColorDefaultBrush");
			else if (tb == DescriptionTextBox) DescriptionWrapper.BorderBrush = (Brush)ThemeResource.GetResource("ControlStrokeColorDefaultBrush");
		}
	}

	private void UpdateSaveButtonState()
	{
		// Keep the button enabled so users can see validation errors on click
		SaveBtn.IsEnabled = true;
	}

	private void CategoryNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
	{
		_isDirty = true;
		Bindings.Update();
		UpdateSaveButtonState();
	}

	private void CancelBtn_Click(object sender, RoutedEventArgs e)
	{
		Hide();
	}

	private void SaveBtn_Click(object sender, RoutedEventArgs e)
	{
		_isDirty = true;
		Bindings.Update();
		UpdateSaveButtonState();

		if (IsNameInvalid) return;

		_result = ContentDialogResult.Primary;
		Hide();
	}

	public new async Task<ContentDialogResult> ShowAsync()
	{
		await base.ShowAsync();
		return _result;
	}
}
