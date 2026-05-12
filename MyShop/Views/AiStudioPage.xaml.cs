using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;

namespace MyShop.Views;

public sealed partial class AiStudioPage : Page
{
    public AiStudioPage()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<AiStudioViewModel>();
        Loaded += AiStudioPage_Loaded;
    }

    private async void AiStudioPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is AiStudioViewModel vm)
        {
            // Auto-scroll when new messages are added or content grows
            ChatItemsControl.SizeChanged += (s, args) => ScrollToBottom();
            await vm.InitializeAsync();
        }
    }

    private void ScrollToBottom()
    {
        ChatScrollViewer.ChangeView(null, ChatScrollViewer.ExtentHeight, null);
    }

    private void OnChatKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter && DataContext is AiStudioViewModel vm)
        {
            if (vm.SendChatCommand.CanExecute(null))
            {
                vm.SendChatCommand.Execute(null);
            }
        }
    }
}
