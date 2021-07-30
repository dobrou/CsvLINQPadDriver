using System.ComponentModel;
using System.Windows.Input;

namespace CsvLINQPadDriver.Wpf
{
    internal class RoutedUICommandEx : RoutedUICommand
    {
        private readonly string? _toolTip;

        public string? ToolTip
        {
            // ReSharper disable once UnusedMember.Global
            get => _toolTip is null ? null : string.Format($"{_toolTip}{(_toolTip.Contains("{0}") ? string.Empty : " ({0})")}", InputGestureText);
            init => _toolTip = value;
        }

        public string InputGestureText { get; init; } = null!;

        public KeyGesture InputGestureAsKeyGesture =>
            InputGestureAs<KeyGesture>();

        public MouseGesture InputGestureAsMouseGesture =>
            InputGestureAs<MouseGesture>();

        private T InputGestureAs<T>() where T : InputGesture =>
            ((T)TypeDescriptor.GetConverter(typeof(T)).ConvertFrom(null!, null!, InputGestureText)!)!;
    }
}
