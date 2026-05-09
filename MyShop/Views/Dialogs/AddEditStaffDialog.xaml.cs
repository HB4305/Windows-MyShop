using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Models;
using MyShop.Repositories;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MyShop.Views.Dialogs;

public sealed partial class AddEditStaffDialog : ContentDialog, INotifyPropertyChanged
{
    private readonly UserRecord? _existingUser;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _emailValidationMessage = string.Empty;
    private string _passwordValidationMessage = string.Empty;

    public AddEditStaffDialog(UserRecord? user = null)
    {
        InitializeComponent();
        _existingUser = user;

        if (_existingUser is not null)
        {
            FormTitle.Text = "Edit Staff";
            FormSubtitle.Text = "Update login details and access level for this staff account.";
            SaveBtnText.Text = "Update Staff";
            PasswordHintText.Text = "Leave the password empty if you want to keep the current password.";

            Email = _existingUser.Email;
        }
        else
        {
            FormTitle.Text = "New Staff";
            FormSubtitle.Text = "Create a new staff account that can sign in to MyShop.";
            SaveBtnText.Text = "Create Staff";
            PasswordHintText.Text = "Password is required for a new staff account.";
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Email
    {
        get => _email;
        set
        {
            SetProperty(ref _email, value);
            ValidateEmail();
        }
    }

    public string Password
    {
        get => _password;
        private set
        {
            SetProperty(ref _password, value);
            ValidatePassword();
        }
    }

    public string EmailValidationMessage
    {
        get => _emailValidationMessage;
        private set => SetProperty(ref _emailValidationMessage, value);
    }

    public string PasswordValidationMessage
    {
        get => _passwordValidationMessage;
        private set => SetProperty(ref _passwordValidationMessage, value);
    }

    public ContentDialogResult Result { get; private set; } = ContentDialogResult.None;

    public StaffFormData GetFormData() => new()
    {
        Id = _existingUser?.Id,
        Email = Email.Trim(),
        Role = "sale",
        Password = Password
    };

    public new async Task<ContentDialogResult> ShowAsync()
    {
        await base.ShowAsync();
        return Result;
    }

    private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
    {
        Password = PasswordInput.Password;
    }

    private void SaveBtn_Click(object sender, RoutedEventArgs e)
    {
        ValidateEmail();
        ValidatePassword();

        if (!string.IsNullOrEmpty(EmailValidationMessage) || !string.IsNullOrEmpty(PasswordValidationMessage))
        {
            return;
        }

        Result = ContentDialogResult.Primary;
        Hide();
    }

    private void CancelBtn_Click(object sender, RoutedEventArgs e)
    {
        Result = ContentDialogResult.None;
        Hide();
    }

    private void ValidateEmail()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            EmailValidationMessage = "Email is required.";
            return;
        }

        EmailValidationMessage = Email.Contains('@')
            ? string.Empty
            : "Please enter a valid email address.";
    }

    private void ValidatePassword()
    {
        if (_existingUser is null && string.IsNullOrWhiteSpace(Password))
        {
            PasswordValidationMessage = "Password is required.";
            return;
        }

        if (!string.IsNullOrWhiteSpace(Password) && Password.Length < 6)
        {
            PasswordValidationMessage = "Password must be at least 6 characters.";
            return;
        }

        PasswordValidationMessage = string.Empty;
    }

    private void SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(storage, value))
        {
            return;
        }

        storage = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
