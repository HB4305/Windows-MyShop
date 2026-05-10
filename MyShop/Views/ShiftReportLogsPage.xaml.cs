using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MyShop.Services;
using MyShop.ViewModels;

namespace MyShop.Views;

public sealed partial class ShiftReportLogsPage : Page
{
    public ShiftReportLogsViewModel ViewModel { get; }

    public ShiftReportLogsPage()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<ShiftReportLogsViewModel>();
        DataContext = ViewModel;

        Loaded += ShiftReportLogsPage_Loaded;

        ViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ViewModel.CurrentPage) || e.PropertyName == nameof(ViewModel.TotalFilteredItems))
            {
                BuildPagination();
            }
        };
    }

    private async void ShiftReportLogsPage_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadLogsCommand.ExecuteAsync(null);
        BuildPagination();
    }

    private void BackToShiftManagement_Click(object sender, RoutedEventArgs e)
    {
        var currentUser = App.Services.GetRequiredService<CurrentUserService>();
        Frame.Navigate(currentUser.IsOwner ? typeof(DashboardPage) : typeof(ShiftManagementPage));
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
                ViewModel.GoToPage(page);
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
        prevBtn.Click += (s, e) => { if (current > 1) { ViewModel.GoToPage(current - 1); BuildPagination(); } };
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
            if (current > 3) PaginationPanel.Children.Add(new TextBlock { Text = "...", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 4, 0), Foreground = (Brush)Resources["TextFillColorSecondaryBrush"] });

            int start = Math.Max(2, current - 1);
            int end = Math.Min(total - 1, current + 1);

            for (int i = start; i <= end; i++)
                AddPageBtn(i, i == current);

            if (current < total - 2) PaginationPanel.Children.Add(new TextBlock { Text = "...", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 4, 0), Foreground = (Brush)Resources["TextFillColorSecondaryBrush"] });
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
        nextBtn.Click += (s, e) => { if (current < total) { ViewModel.GoToPage(current + 1); BuildPagination(); } };
        PaginationPanel.Children.Add(nextBtn);
    }
}

