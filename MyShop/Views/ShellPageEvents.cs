namespace MyShop.Views;

/// <summary>
/// Navigation events from ShellPage.
/// </summary>
public static class ShellPageEvents
{
    public static event Action? OnLogout;
    public static void RaiseLogout() => OnLogout?.Invoke();
}
