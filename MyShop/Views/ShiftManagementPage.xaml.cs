using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MyShop.ViewModels;
using MyShop.Views.Dialogs;

namespace MyShop.Views;

public sealed partial class ShiftManagementPage : Page
{
    public ShiftManagementPage()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<ShiftViewModel>();
        if (ViewModel is not null)
        {
            ViewModel.PromptNegativeDiscrepancyReasonAsync = PromptNegativeDiscrepancyReasonAsync;
        }

        Loaded += ShiftManagementPage_Loaded;
    }

    private ShiftViewModel? ViewModel => DataContext as ShiftViewModel;

    private async void ShiftManagementPage_Loaded(object sender, RoutedEventArgs e)
    {
        ViewReportLogsButton.Visibility = Visibility.Collapsed;

        if (ViewModel is not null)
        {
            await ViewModel.LoadActiveShiftCommand.ExecuteAsync(null);
        }
    }

    private async Task<string?> PromptNegativeDiscrepancyReasonAsync(decimal discrepancy, string? existingReason)
    {
        var dialog = new NegativeDiscrepancyReasonDialog(discrepancy, existingReason)
        {
            XamlRoot = XamlRoot
        };

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary
            ? dialog.ReasonText
            : null;
    }

    private void ViewReportLogs_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(ShiftReportLogsPage));
    }

    private void StartingCashTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        StartingCashWrapper.BorderBrush = (Brush)Application.Current.Resources["AppPurpleBrush"];
    }

    private void StartingCashTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        StartingCashWrapper.BorderBrush = (Brush)Application.Current.Resources["ControlStrokeColorDefaultBrush"];
    }

    private void ActualCashTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        ActualCashWrapper.BorderBrush = (Brush)Application.Current.Resources["AppPurpleBrush"];
    }

    private void ActualCashTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        ActualCashWrapper.BorderBrush = (Brush)Application.Current.Resources["ControlStrokeColorDefaultBrush"];
    }

    private void NotesTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        NotesWrapper.BorderBrush = (Brush)Application.Current.Resources["AppPurpleBrush"];
    }

    private void NotesTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        NotesWrapper.BorderBrush = (Brush)Application.Current.Resources["ControlStrokeColorDefaultBrush"];
    }
}
