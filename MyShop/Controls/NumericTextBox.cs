using System.Text.RegularExpressions;
using Microsoft.UI.Xaml.Controls;

namespace MyShop.Controls;

/// <summary>
/// A TextBox that only accepts integer numbers (0-9).
/// Uses BeforeTextChanging to block invalid characters BEFORE they are inserted.
/// </summary>
public class NumericTextBox : TextBox
{
    public NumericTextBox()
    {
        BeforeTextChanging += OnBeforeTextChanging;
    }

    private void OnBeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
    {
        // If the new text contains any non-digit, cancel the change
        if (!string.IsNullOrEmpty(args.NewText) && !Regex.IsMatch(args.NewText, @"^\d*$"))
        {
            args.Cancel = true;
        }
    }
}
