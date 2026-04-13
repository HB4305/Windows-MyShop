using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Repositories;
using MyShop.Services;

namespace MyShop.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsManager _settingsManager;
    private readonly UserRepository _userRepository;
    private readonly CurrentUserService _currentUserService;

    public SettingsViewModel(
        SettingsManager settingsManager,
        UserRepository userRepository,
        CurrentUserService currentUserService)
    {
        _settingsManager = settingsManager;
        _userRepository = userRepository;
        _currentUserService = currentUserService;

        ItemsPerPageOptions = new ObservableCollection<int> { 5, 10, 15, 20 };

        var saved = _settingsManager.GetItemsPerPage();
        SelectedItemsPerPage = ItemsPerPageOptions.Contains(saved) ? saved : 5;

        RememberLastActivity = _settingsManager.GetRememberLastActivity();

        // Load user list when owner
        if (_currentUserService.IsOwner)
            _ = LoadUsersAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // App Settings
    // ═══════════════════════════════════════════════════════════════════════

    public ObservableCollection<int> ItemsPerPageOptions { get; }

    [ObservableProperty]
    private int _selectedItemsPerPage;

    partial void OnSelectedItemsPerPageChanged(int value)
    {
        // Auto-save pagination preference when user changes the value
        _settingsManager.SetItemsPerPage(value);
    }

    [ObservableProperty]
    private bool _rememberLastActivity;

    partial void OnRememberLastActivityChanged(bool value)
    {
        // Auto-save RememberLastActivity preference immediately when toggled
        _settingsManager.SetRememberLastActivity(value);
        NewUserSuccessMessage = value
            ? "Last activity will be restored on next startup."
            : "Last activity tracking has been disabled.";
        // Auto-clear last activity when disabled
        if (!value)
        {
            _settingsManager.SetLastActivity(null);
        }
    }

    [RelayCommand]
    private void SaveChanges()
    {
        _settingsManager.SetItemsPerPage(SelectedItemsPerPage);
        _settingsManager.SetRememberLastActivity(RememberLastActivity);
    }

    [RelayCommand]
    private void Reset()
    {
        SelectedItemsPerPage = 5;
        RememberLastActivity = false;
        _settingsManager.SetItemsPerPage(SelectedItemsPerPage);
        _settingsManager.SetRememberLastActivity(RememberLastActivity);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // User Management (owner only)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>Owner only can manage users.</summary>
    public bool IsOwnerView => _currentUserService.IsOwner;

    [ObservableProperty]
    private ObservableCollection<UserRecord> _users = new();

    [ObservableProperty]
    private bool _isLoadingUsers;

    [ObservableProperty]
    private string? _userErrorMessage;

    [ObservableProperty]
    private string _newUserEmail = string.Empty;

    [ObservableProperty]
    private string _newUserPassword = string.Empty;

    [ObservableProperty]
    private string? _newUserErrorMessage;

    [ObservableProperty]
    private string? _newUserSuccessMessage;

    [RelayCommand]
    public async Task LoadUsersAsync()
    {
        try
        {
            IsLoadingUsers = true;
            UserErrorMessage = null;

            var userList = new List<UserRecord>();
            Users = new ObservableCollection<UserRecord>(userList);
        }
        catch (Exception ex)
        {
            UserErrorMessage = ex.Message;
        }
        finally
        {
            IsLoadingUsers = false;
        }
    }

    /// <summary>
    /// Create a new sale user. Owner only.
    /// </summary>
    [RelayCommand]
    public async Task CreateSaleUserAsync()
    {
        NewUserErrorMessage = null;
        NewUserSuccessMessage = null;

        if (!_currentUserService.IsOwner)
        {
            NewUserErrorMessage = "Only owner can create new users.";
            return;
        }

        if (string.IsNullOrWhiteSpace(NewUserEmail))
        {
            NewUserErrorMessage = "Please enter email.";
            return;
        }
        if (string.IsNullOrWhiteSpace(NewUserPassword) || NewUserPassword.Length < 6)
        {
            NewUserErrorMessage = "Password must be at least 6 characters.";
            return;
        }

        try
        {
            var user = await _userRepository.CreateAsync(
                NewUserEmail.Trim().ToLowerInvariant(),
                NewUserPassword,
                "sale");

            if (user == null)
            {
                NewUserErrorMessage = "Email already exists or failed to create user.";
                return;
            }

            Users.Add(user);
            NewUserSuccessMessage = $"User '{user.Email}' created successfully!";
            NewUserEmail = string.Empty;
            NewUserPassword = string.Empty;
        }
        catch (Exception ex)
        {
            NewUserErrorMessage = $"Error: {ex.Message}";
        }
    }
}
