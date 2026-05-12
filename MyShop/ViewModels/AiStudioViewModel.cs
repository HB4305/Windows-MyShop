using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models.Ai;
using MyShop.Models.ReportModels;
using MyShop.Services;

namespace MyShop.ViewModels;

public partial class AiStudioViewModel : ObservableObject
{
    private readonly IAiService _aiService;
    private readonly SportItemService _sportItemService;
    private readonly ReportService _reportService;
    private readonly AiLogService _aiLogService;

    public AiStudioViewModel(
        IAiService aiService,
        SportItemService sportItemService,
        ReportService reportService,
        AiLogService aiLogService)
    {
        _aiService = aiService;
        _sportItemService = sportItemService;
        _reportService = reportService;
        _aiLogService = aiLogService;
    }

    [ObservableProperty]
    private ObservableCollection<AiChatMessage> _chatMessages = [];

    [ObservableProperty]
    private string _chatInput = string.Empty;

    [ObservableProperty]
    private bool _isChatBusy;

    [ObservableProperty]
    private bool _isThinking;

    [ObservableProperty]
    private string _chatError = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _suggestedQuestions = 
    [
        "What are the top sellers this week?",
        "Which items are low in stock?",
        "Show me a summary of last month's profit.",
        "How can I improve my pricing strategy?"
    ];

    public async Task InitializeAsync()
    {
        if (ChatMessages.Count == 0)
        {
            await ShowWelcomeMessageAsync();
        }
    }

    [RelayCommand]
    public void ClearHistory()
    {
        ChatMessages.Clear();
        _ = ShowWelcomeMessageAsync();
    }

    private async Task ShowWelcomeMessageAsync()
    {
        var welcomeText = "Hello! I am your MyShop AI Studio assistant. 🚀\n\nI can help you analyze sales trends, identify low-stock items, and optimize your retail strategy using real-time data from your shop.\n\nHow can I assist you today?";
        
        var assistantMessage = new AiChatMessage
        {
            Role = "assistant",
            Content = string.Empty
        };
        ChatMessages.Add(assistantMessage);

        int index = 0;
        while (index < welcomeText.Length)
        {
            int count = Math.Min(3, welcomeText.Length - index);
            assistantMessage.Content += welcomeText.Substring(index, count);
            index += count;
            await Task.Delay(10);
        }
    }

    [RelayCommand]
    private async Task ApplySuggestedQuestionAsync(string question)
    {
        ChatInput = question;
        await SendChatAsync();
    }

    private System.Threading.CancellationTokenSource? _cts;

    [RelayCommand]
    public void StopChat()
    {
        _cts?.Cancel();
    }

    [RelayCommand]
    public async Task SendChatAsync()
    {
        if (string.IsNullOrWhiteSpace(ChatInput) || IsChatBusy)
            return;

        _cts?.Cancel();
        _cts = new System.Threading.CancellationTokenSource();
        var token = _cts.Token;

        var userMessage = new AiChatMessage
        {
            Role = "user",
            Content = ChatInput.Trim(),
            TimestampUtc = DateTime.UtcNow
        };

        ChatMessages.Add(userMessage);
        ChatInput = string.Empty;

        var sw = Stopwatch.StartNew();
        var logEntry = new AiLogEntry { Operation = "ai_chat" };

        try
        {
            IsChatBusy = true;
            IsThinking = true; // Start thinking indicator
            ChatError = string.Empty;

            var context = await BuildChatContextAsync();
            var prompt = $@"Context:\n{context}\n\nUser question:\n{userMessage.Content}\n\nRespond with concise, helpful guidance. If data is missing, say what data is needed.";

            var response = await _aiService.RunPromptAsync(
                "You are an assistant for a sports retail management system.",
                prompt,
                token);

            var fullResponse = NormalizeChatResponse(response);
            IsThinking = false; // Stop thinking indicator as soon as we have the response

            var assistantMessage = new AiChatMessage
            {
                Role = "assistant",
                Content = string.Empty,
                TimestampUtc = DateTime.UtcNow
            };
            ChatMessages.Add(assistantMessage);

            // Typewriter effect
            int index = 0;
            int chunkSize = 2;
            while (index < fullResponse.Length)
            {
                token.ThrowIfCancellationRequested();
                int remaining = fullResponse.Length - index;
                int count = Math.Min(chunkSize, remaining);
                assistantMessage.Content += fullResponse.Substring(index, count);
                index += count;
                await Task.Delay(15, token); 
            }

            logEntry.Success = true;
        }
        catch (OperationCanceledException)
        {
            // User stopped the chat, just end gracefully
            logEntry.Success = true;
            logEntry.Error = "Stopped by user";
        }
        catch (Exception ex)
        {
            ChatError = $"Chat error: {ex.Message}";
            logEntry.Success = false;
            logEntry.Error = ex.Message;
            
            ChatMessages.Add(new AiChatMessage 
            { 
                Role = "assistant", 
                Content = "I'm sorry, I encountered an error while processing your request. Please try again later.",
                TimestampUtc = DateTime.UtcNow
            });
        }
        finally
        {
            sw.Stop();
            logEntry.DurationMs = sw.ElapsedMilliseconds;
            await SafeLogAsync(logEntry);
            IsChatBusy = false;
            IsThinking = false;
        }
    }

    private async Task<string> BuildChatContextAsync()
    {
        try 
        {
            var period = ReportService.CreatePeriodSelection(ReportPeriod.Week);
            var topProducts = await _reportService.GetTopPerformingProductsAsync(period, limit: 3);
            var lowStock = await _sportItemService.GetLowStockProductsAsync(threshold: 5, limit: 3);

            var sb = new StringBuilder();
            sb.AppendLine("Top products this week:");
            foreach (var p in topProducts)
            {
                sb.AppendLine($"- {p.ProductName} (qty {p.TotalQuantitySold}, revenue {p.GrossRevenue})");
            }
            sb.AppendLine("Low stock alerts:");
            foreach (var p in lowStock)
            {
                sb.AppendLine($"- {p.Name} (stock {p.StockQuantity})");
            }

            return sb.ToString();
        }
        catch 
        {
            return "Note: Real-time shop data could not be retrieved at this moment.";
        }
    }

    private static string NormalizeChatResponse(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var normalized = input.Replace("\r\n", "\n");
        normalized = normalized.Replace("**", string.Empty);

        var lines = normalized.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            var trimmed = lines[i].TrimStart();
            if (trimmed.StartsWith("* "))
            {
                var prefixLen = lines[i].Length - trimmed.Length;
                lines[i] = new string(' ', prefixLen) + "- " + trimmed.Substring(2);
            }
        }

        return string.Join(Environment.NewLine, lines).Trim();
    }

    private async Task SafeLogAsync(AiLogEntry entry)
    {
        try
        {
            await _aiLogService.LogAsync(entry);
        }
        catch
        {
            // Ignore logging errors.
        }
    }
}
