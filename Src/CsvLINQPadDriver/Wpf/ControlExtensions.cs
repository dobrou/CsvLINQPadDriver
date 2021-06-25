using System;
using System.Windows;
using System.Windows.Controls;

namespace CsvLINQPadDriver.Wpf
{
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
    }
}
