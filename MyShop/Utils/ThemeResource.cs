using Microsoft.UI.Xaml;

namespace MyShop;

public static class ThemeResource
{
    /// <summary>
    /// Helper to retrieve a resource from the application resources or theme dictionaries.
    /// </summary>
    public static object GetResource(string key)
    {
        // Try application resources first
        if (Application.Current.Resources.TryGetValue(key, out var resource))
        {
            return resource;
        }

        // Try theme dictionaries (e.g., for system brushes that change with Light/Dark mode)
        var theme = Application.Current.RequestedTheme.ToString();
        if (Application.Current.Resources.ThemeDictionaries.TryGetValue(theme, out var themeDict) && 
            themeDict is ResourceDictionary dict && 
            dict.TryGetValue(key, out var themeResource))
        {
            return themeResource;
        }

        // Fallback for Default theme dictionary
        if (Application.Current.Resources.ThemeDictionaries.TryGetValue("Default", out var defaultDict) && 
            defaultDict is ResourceDictionary dDict && 
            dDict.TryGetValue(key, out var defaultResource))
        {
            return defaultResource;
        }

        return null;
    }
}
