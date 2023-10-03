using System.Windows;

namespace CsvLINQPadDriver.Wpf;

public class DriverProperties : UIElement
{
    public static readonly DependencyProperty PropertyProperty =
        DependencyProperty.RegisterAttached(
            "Property",
            typeof(string),
            typeof(DriverProperties),
            new FrameworkPropertyMetadata(string.Empty)
        );

    public static string? GetProperty(UIElement target) =>
        (string?) target.GetValue(PropertyProperty);

    public static void SetProperty(UIElement target, string value) =>
        target.SetValue(PropertyProperty, value);
}
