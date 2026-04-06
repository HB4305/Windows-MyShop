namespace MyShop.Views;

/// <summary>
/// Sự kiện điều hướng từ ShellPage.
/// </summary>
public static class ShellPageEvents
{
    public static event Action? OnLogout;
    public static void RaiseLogout() => OnLogout?.Invoke();
}
