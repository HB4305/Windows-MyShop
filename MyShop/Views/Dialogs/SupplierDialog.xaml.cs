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
            ErrorText.Visibility = Visibility.Collapsed;

            var name = NameTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                ErrorText.Text = "Supplier name cannot be empty.";
                ErrorText.Visibility = Visibility.Visible;
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
            ErrorText.Text = $"Error: {ex.Message}";
            ErrorText.Visibility = Visibility.Visible;
        }
    }
}
