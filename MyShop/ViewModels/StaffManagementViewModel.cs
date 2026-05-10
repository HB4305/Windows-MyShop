using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;
using MyShop.Repositories;
using MyShop.Services;

namespace MyShop.ViewModels;

public partial class StaffManagementViewModel : ObservableObject
{
    private readonly UserRepository _userRepository;
    private readonly CurrentUserService _currentUserService;
    private readonly SettingsManager _settingsManager;

    public StaffManagementViewModel(UserRepository userRepository, CurrentUserService currentUserService, SettingsManager settingsManager)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _settingsManager = settingsManager;

        _pageSize = _settingsManager.GetItemsPerPage();
    }

    [ObservableProperty] private ObservableCollection<UserRecord> _staffMembers = [];
    [ObservableProperty] private ObservableCollection<UserRecord> _filteredStaffMembers = [];
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _isLoading;

    // Pagination properties for UI synchronization
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPages))]
    [NotifyPropertyChangedFor(nameof(DisplayFrom))]
    [NotifyPropertyChangedFor(nameof(DisplayTo))]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPages))]
    [NotifyPropertyChangedFor(nameof(DisplayFrom))]
    [NotifyPropertyChangedFor(nameof(DisplayTo))]
    private int _totalItems;

    public int TotalPages => (TotalItems + PageSize - 1) / Math.Max(1, PageSize);
    public int DisplayFrom => TotalItems == 0 ? 0 : (CurrentPage - 1) * PageSize + 1;
    public int DisplayTo => Math.Min(CurrentPage * PageSize, TotalItems);

    public int TotalStaffCount => StaffMembers.Count;
    public int SaleCount => StaffMembers.Count(user => NormalizeRole(user.Role) == "sale");

    partial void OnSearchTextChanged(string value)
    {
        CurrentPage = 1;
        ApplyFilter();
    }

    [RelayCommand]
    public async Task LoadStaffAsync()
    {
        PageSize = Math.Max(1, _settingsManager.GetItemsPerPage());
        if (!_currentUserService.IsOwner)
        {
            StatusMessage = "Only owner accounts can manage staff.";
            StaffMembers = [];
            FilteredStaffMembers = [];
            NotifyStatsChanged();
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = string.Empty;

            var users = await _userRepository.GetAllAsync();
            var saleUsers = users
                .Where(user => NormalizeRole(user.Role) == "sale")
                .ToList();

            StaffMembers = new ObservableCollection<UserRecord>(saleUsers);
            ApplyFilter();
            NotifyStatsChanged();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load staff: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CreateStaffAsync(StaffFormData? form)
    {
        if (form is null) return;

        var validationError = Validate(form, requirePassword: true);
        if (validationError is not null)
        {
            StatusMessage = validationError;
            return;
        }

        try
        {
            IsLoading = true;
            var created = await _userRepository.CreateAsync(
                form.Email.Trim().ToLowerInvariant(),
                form.Password,
                NormalizeRole(form.Role));

            if (created is null)
            {
                StatusMessage = "Failed to create staff. The email may already exist.";
                return;
            }

            await LoadStaffAsync();
            StatusMessage = $"Created staff account '{created.Email}'.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to create staff: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task UpdateStaffAsync(StaffFormData? form)
    {
        if (form?.Id is null) return;

        var validationError = Validate(form, requirePassword: false);
        if (validationError is not null)
        {
            StatusMessage = validationError;
            return;
        }

        var existing = StaffMembers.FirstOrDefault(user => user.Id == form.Id.Value);
        if (existing is null)
        {
            StatusMessage = "This staff account no longer exists.";
            return;
        }

        try
        {
            IsLoading = true;

            var updated = await _userRepository.UpdateAsync(
                existing.Id,
                form.Email,
                "sale",
                string.IsNullOrWhiteSpace(form.Password) ? null : form.Password);

            if (updated is null)
            {
                StatusMessage = "Failed to update staff. The email may already exist.";
                return;
            }

            await LoadStaffAsync();
            StatusMessage = $"Updated staff account '{updated.Email}'.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to update staff: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task DeleteStaffAsync(UserRecord? user)
    {
        if (user is null) return;

        if (NormalizeRole(user.Role) != "sale")
        {
            StatusMessage = "Only SALE accounts can be managed here.";
            return;
        }

        try
        {
            IsLoading = true;
            await _userRepository.DeleteAsync(user.Id);
            await LoadStaffAsync();
            StatusMessage = $"Deleted staff account '{user.Email}'.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to delete staff: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void NextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            ApplyFilter();
        }
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            ApplyFilter();
        }
    }

    private void ApplyFilter()
    {
        var keyword = SearchText.Trim();
        IEnumerable<UserRecord> filtered = StaffMembers;

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            filtered = filtered.Where(user =>
                user.Email.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                (user.Role?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        var allFiltered = filtered.ToList();
        TotalItems = allFiltered.Count;

        // Apply client-side pagination
        var paged = allFiltered
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        FilteredStaffMembers = new ObservableCollection<UserRecord>(paged);
    }

    private void NotifyStatsChanged()
    {
        OnPropertyChanged(nameof(TotalStaffCount));
        OnPropertyChanged(nameof(SaleCount));
    }

    private static string? Validate(StaffFormData form, bool requirePassword)
    {
        if (string.IsNullOrWhiteSpace(form.Email)) return "Email is required.";
        if (!form.Email.Contains('@')) return "Please enter a valid email address.";
        if (requirePassword && string.IsNullOrWhiteSpace(form.Password)) return "Password is required for new staff accounts.";
        if (!string.IsNullOrWhiteSpace(form.Password) && form.Password.Length < 6) return "Password must be at least 6 characters.";
        return null;
    }

    private static string NormalizeRole(string? role)
        => role?.Trim().ToLowerInvariant() == "owner" ? "owner" : "sale";
}
