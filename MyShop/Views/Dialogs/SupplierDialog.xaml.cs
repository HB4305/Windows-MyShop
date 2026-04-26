using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using MyShop.Models;
using MyShop.Repositories;

namespace MyShop.Views.Dialogs;

public sealed partial class SupplierDialog : ContentDialog
{
    private readonly SupplierRepository _repo;
    private readonly Supplier? _existingSupplier;

    public SupplierDialog(Supplier? supplier)
    {
        this.InitializeComponent();
        _repo = App.Services.GetRequiredService<SupplierRepository>();
        _existingSupplier = supplier;

        if (_existingSupplier != null)
        {
            Title = "Edit Supplier";
            NameTextBox.Text = _existingSupplier.Name;
            PhoneTextBox.Text = _existingSupplier.ContactPhone ?? "";
            TypeTextBox.Text = _existingSupplier.SupplierType ?? "";
        }
        else
        {
            Title = "Add Supplier";
        }
    }

    private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var deferral = args.GetDeferral();
        try
        {
            ErrorText.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;

            var name = NameTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                ErrorText.Text = "Supplier name cannot be empty.";
                ErrorText.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                args.Cancel = true;
                return;
            }

            try
            {
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
            }
            catch (System.Exception ex)
            {
                ErrorText.Text = $"Error: {ex.Message}";
                ErrorText.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                args.Cancel = true;
            }
        }
        finally
        {
            deferral.Complete();
        }
    }
}
