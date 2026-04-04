using System.Text.RegularExpressions;
using Microsoft.UI.Xaml.Controls;

namespace MyShop.Controls;

/// <summary>
/// A TextBox that only accepts decimal numbers (digits, one dot, optional leading minus).
/// Uses BeforeTextChanging to block invalid characters BEFORE they are inserted.
/// </summary>
public class DecimalTextBox : TextBox
{
    public DecimalTextBox()
    {
        BeforeTextChanging += OnBeforeTextChanging;
    }

    private void OnBeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
    {
        if (string.IsNullOrEmpty(args.NewText))
            return;

        // Allow: digits, one dot, minus at start only
        if (!IsValidDecimalInput(args.NewText))
        {
            args.Cancel = true;
        }
    }

    private static bool IsValidDecimalInput(string input)
    {
        // Must contain only digits, one dot, optional leading minus
        if (!Regex.IsMatch(input, @"^[\d.\-]*$"))
            return false;

        // Only one minus, and only at start
        var minusCount = 0;
        for (var i = 0; i < input.Length; i++)
        {
            if (input[i] == '-')
            {
                if (i != 0) return false;
                minusCount++;
                if (minusCount > 1) return false;
            }
        }

        // Only one dot total
        var dotCount = 0;
        foreach (var c in input)
            if (c == '.') dotCount++;
        if (dotCount > 1) return false;

        return true;
    }
}
