using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace CsvLINQPadDriver.Wpf
{
    public class Settings : UIElement
    {
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.RegisterAttached(
                "Name",
                typeof(string),
                typeof(Settings),
                new FrameworkPropertyMetadata(string.Empty)
            );

        public static string? GetName(UIElement target) =>
            (string?) target.GetValue(NameProperty);

        public static void SetName(UIElement target, string value) =>
            target.SetValue(NameProperty, value);
    }

    public static class Utils
    {
        public static IEnumerable<DependencyObject> EnumChildren(DependencyObject? parent, Predicate<FrameworkElement> predicate)
        {
            switch (parent)
            {
                case null:
                    yield break;
                case FrameworkElement frameworkElement when predicate(frameworkElement):
                    yield return frameworkElement;
                    break;
            }

            if (parent is FrameworkElement element)
            {
                element.ApplyTemplate();
            }

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                foreach (var a in EnumChildren(child, predicate))
                {
                    yield return a;
                }
            }
        }
    }
}