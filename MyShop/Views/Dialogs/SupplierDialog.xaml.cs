using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Models;
using MyShop.Repositories;

namespace MyShop.Views.Dialogs;

public sealed partial class SupplierDialog : ContentDialog
{
    private readonly SupplierRepository _repo;
    private readonly Supplier? _existingSupplier;
    private ContentDialogResult _result = ContentDialogResult.None;

    public SupplierDialog(Supplier? supplier)
    {
        this.InitializeComponent();
        _repo = App.Services.GetRequiredService<SupplierRepository>();
        _existingSupplier = supplier;

        if (_existingSupplier != null)
        {
            DialogTitleText.Text = "Edit Supplier";
            NameTextBox.Text = _existingSupplier.Name;
            PhoneTextBox.Text = _existingSupplier.ContactPhone ?? "";
            TypeTextBox.Text = _existingSupplier.SupplierType ?? "";
        }
        else
        {
            DialogTitleText.Text = "Add Supplier";
        }
    }

    public new async Task<ContentDialogResult> ShowAsync()
    {
        await base.ShowAsync();
        return _result;
    }

    private void CancelBtn_Click(object sender, RoutedEventArgs e)
    {
        _result = ContentDialogResult.None;
        Hide();
    }

    private async void SaveBtn_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            NameErrorText.Visibility = Visibility.Collapsed;
            var name = NameTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                NameErrorText.Text = "Supplier name cannot be empty.";
                NameErrorText.Visibility = Visibility.Visible;
                return;
            }

            if (_existingSupplier != null)
            {
                _existingSupplier.Name = name;
                _existingSupplier.ContactPhone = PhoneTextBox.Text.Trim();
                _existingSupplier.SupplierType = TypeTextBox.Text.Trim();
                await _repo.UpdateAsync(_existingSupplier);
            }
            else
            {
                var newSupplier = new Supplier
                {
                    Name = name,
                    ContactPhone = PhoneTextBox.Text.Trim(),
                    SupplierType = TypeTextBox.Text.Trim()
                };
                await _repo.CreateAsync(newSupplier);
            }

            _result = ContentDialogResult.Primary;
            Hide();
        }
        catch (System.Exception ex)
        {
            NameErrorText.Text = $"Error: {ex.Message}";
            NameErrorText.Visibility = Visibility.Visible;
        }
    }

    private void TextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb)
        {
            if (tb == NameTextBox) NameWrapper.BorderBrush = (Brush)ThemeResource.GetResource("AppPurpleBrush");
            else if (tb == PhoneTextBox) PhoneWrapper.BorderBrush = (Brush)ThemeResource.GetResource("AppPurpleBrush");
            else if (tb == TypeTextBox) TypeWrapper.BorderBrush = (Brush)ThemeResource.GetResource("AppPurpleBrush");
        }
    }

    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb)
        {
            if (tb == NameTextBox) NameWrapper.BorderBrush = (Brush)ThemeResource.GetResource("ControlStrokeColorDefaultBrush");
            else if (tb == PhoneTextBox) PhoneWrapper.BorderBrush = (Brush)ThemeResource.GetResource("ControlStrokeColorDefaultBrush");
            else if (tb == TypeTextBox) TypeWrapper.BorderBrush = (Brush)ThemeResource.GetResource("ControlStrokeColorDefaultBrush");
        }
    }
}
