using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using CsvLINQPadDriver.Extensions;

namespace CsvLINQPadDriver.Wpf.Extensions;

internal static class ControlExtensions
{
    private static readonly char[] NewLineChars = Environment.NewLine.ToCharArray();

    public static string GetLineTextAtCaretIndex(this TextBox textBox)
    {
        var caretIndex = textBox.CaretIndex;
        var text = textBox.Text;
        var textLength = text.Length;

        return text[ScanLeft()..ScanRight()];

        int ScanLeft()
        {
            var pos = GetCurrentPos();

            for (var newLineCheck = NewLineChars.Length; --newLineCheck != 0 && pos >= 0 && IsNewLine(pos); pos--)
            {
                // Skip empty lines.
            }

            for (; pos >= 0; pos--)
            {
                if (IsNewLine(pos))
                {
                    return pos;
                }
            }

            return 0;
        }

        int ScanRight()
        {
            var pos = GetCurrentPos();

            for (; pos < textLength; pos++)
            {
                if (IsNewLine(pos))
                {
                    return pos;
                }
            }

            return textLength;
        }

        int GetCurrentPos() =>
            caretIndex == 0 || caretIndex < textLength
                ? caretIndex
                : textLength - 1;

        bool IsNewLine(int pos) =>
            textLength > pos && Array.IndexOf(NewLineChars, text[pos]) != -1;
    }

    public static void UpdateTargetBinding(this FrameworkElement frameworkElement, DependencyProperty dependencyProperty) =>
        frameworkElement.GetBindingExpression(dependencyProperty)!.UpdateTarget();

    public static string ReplaceHotKeyChar(this ContentControl contentControl, string? newChar = null) =>
        ((string)contentControl.Content).ReplaceHotKeyChar(newChar);

    public sealed record EnumChildrenResult<T>(FrameworkElement Element, T Value);

    public static IEnumerable<EnumChildrenResult<T>> EnumChildren<T>(this DependencyObject? parent, Func<FrameworkElement, EnumChildrenResult<T>?> getResult)
    {
        if (parent is null)
        {
            yield break;
        }

        if (parent is FrameworkElement frameworkElement)
        {
            var result = getResult(frameworkElement);
            if (result is not null)
            {
                yield return result;
            }

            frameworkElement.ApplyTemplate();
        }

        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            foreach (var result in EnumChildren(child, getResult))
            {
                yield return result;
            }
        }
    }
}
