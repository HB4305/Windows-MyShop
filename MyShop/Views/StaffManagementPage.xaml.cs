using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Models;
using MyShop.Repositories;
using MyShop.ViewModels;
using MyShop.Views.Dialogs;

namespace MyShop.Views;

public sealed partial class StaffManagementPage : Page
{
    public StaffManagementViewModel ViewModel { get; }

    public StaffManagementPage()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<StaffManagementViewModel>();
        this.DataContext = ViewModel;
        Loaded += StaffManagementPage_Loaded;

        // Listen for property changes to rebuild pagination
        ViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ViewModel.CurrentPage) || 
                e.PropertyName == nameof(ViewModel.TotalItems))
            {
                BuildPagination();
            }
        };
    }

    private async void StaffManagementPage_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadStaffAsync();
        BuildPagination();
    }

    private async void AddStaff_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AddEditStaffDialog(null)
        {
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            await ViewModel.LoadStaffAsync();
        }
    }

    private async void EditStaff_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is UserRecord staff)
        {
            var dialog = new AddEditStaffDialog(staff)
            {
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await ViewModel.LoadStaffAsync();
            }
        }
    }

    private async void DeleteStaff_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is UserRecord staff)
        {
            var dialog = new ConfirmationDialog(
                "Delete Staff Account",
                $"Are you sure you want to delete the staff account for {staff.Email}?");
            dialog.XamlRoot = this.XamlRoot;

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await ViewModel.DeleteStaffAsync(staff);
            }
        }
    }

    private void BuildPagination()
    {
        if (PaginationPanel == null) return;
        PaginationPanel.Children.Clear();

        var total = ViewModel.TotalPages;
        var current = ViewModel.CurrentPage;

        void AddPageBtn(int page, bool isActive)
        {
            var btn = new Button
            {
                Content = page.ToString(),
                MinWidth = 32,
                Height = 32,
                Padding = new Thickness(8, 4, 8, 4),
                FontSize = 13,
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(2, 0, 2, 0),
            };

            if (isActive)
            {
                btn.Style = (Style)Resources["PageBtnActive"];
            }
            else
            {
                btn.Style = (Style)Resources["PageBtn"];
            }

            btn.Click += (s, e) =>
            {
                ViewModel.CurrentPage = page;
                _ = ViewModel.LoadStaffAsync(); 
                BuildPagination();
            };

            PaginationPanel.Children.Add(btn);
        }

        // Prev button
        var prevBtn = new Button
        {
            Content = new TextBlock { Text = "Prev", FontSize = 13 },
            MinWidth = 52,
            Height = 32,
            Padding = new Thickness(8, 4, 8, 4),
            Style = (Style)Resources["PageBtn"],
        };
        prevBtn.Click += (s, e) => { if (current > 1) { ViewModel.CurrentPage--; _ = ViewModel.LoadStaffAsync(); BuildPagination(); } };
        PaginationPanel.Children.Add(prevBtn);

        // Page numbers logic (smart ellipsis)
        if (total <= 7)
        {
            for (int i = 1; i <= total; i++)
                AddPageBtn(i, i == current);
        }
        else
        {
            AddPageBtn(1, current == 1);
            if (current > 3) PaginationPanel.Children.Add(new TextBlock { Text = "...", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 4, 0), Foreground = (Microsoft.UI.Xaml.Media.Brush)Resources["TextFillColorSecondaryBrush"] });

            int start = Math.Max(2, current - 1);
            int end = Math.Min(total - 1, current + 1);

            for (int i = start; i <= end; i++)
                AddPageBtn(i, i == current);

            if (current < total - 2) PaginationPanel.Children.Add(new TextBlock { Text = "...", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 4, 0), Foreground = (Microsoft.UI.Xaml.Media.Brush)Resources["TextFillColorSecondaryBrush"] });
            AddPageBtn(total, current == total);
        }

        // Next button
        var nextBtn = new Button
        {
            Content = new TextBlock { Text = "Next", FontSize = 13 },
            MinWidth = 52,
            Height = 32,
            Padding = new Thickness(8, 4, 8, 4),
            Style = (Style)Resources["PageBtn"],
        };
        nextBtn.Click += (s, e) => { if (current < total) { ViewModel.CurrentPage++; _ = ViewModel.LoadStaffAsync(); BuildPagination(); } };
        PaginationPanel.Children.Add(nextBtn);
    }
}

