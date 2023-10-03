using System;
using System.Windows;
using System.Windows.Input;

using CsvLINQPadDriver.Wpf;

namespace CsvLINQPadDriver;

internal partial class ConnectionDialog
{
    public const string ConfirmCheck   = "check";
    public const string ConfirmUncheck = "uncheck";

    public const Visibility NetCoreOnlyVisibility =
#if NETCOREAPP
        Visibility.Visible;
#else
        Visibility.Collapsed;
#endif

    // ReSharper disable StringLiteralTypo
    public static readonly RoutedUICommandEx AddFilesCommand = new()
    {
        Text             = "Add f_iles",
        InputGestureText = "Ctrl+O",
        ToolTip          = "Add files"
    };

    public static readonly KeyGesture AddFilesCommandKeyGesture = AddFilesCommand.InputGestureAsKeyGesture;

    public static readonly RoutedUICommandEx AddFoldersCommand = new()
    {
        Text             = "Add f_olders",
        InputGestureText = "Ctrl+Shift+O",
        ToolTip          = "Add folders"
    };

    public static readonly KeyGesture AddFoldersCommandKeyGesture = AddFoldersCommand.InputGestureAsKeyGesture;

    public static readonly RoutedUICommandEx AddFoldersWithSubfoldersCommand = new()
    {
        Text             = "Add folders with _sub-folders",
        InputGestureText = "Ctrl+Shift+Alt+O",
        ToolTip          = "Add folders with sub-folders"
    };

    public static readonly KeyGesture AddFoldersWithSubfoldersCommandKeyGesture = AddFoldersWithSubfoldersCommand.InputGestureAsKeyGesture;

    public static readonly RoutedUICommandEx DeleteCommand = new()
    {
        Text             = "_Delete",
        InputGestureText = "Del"
    };

    public static readonly RoutedUICommandEx SelectAllCommand = new()
    {
        Text             = "Select _all",
        InputGestureText = "Ctrl+A"
    };

    public static readonly RoutedUICommandEx ClearCommand = new()
    {
        Text             = "C_lear",
        InputGestureText = "Ctrl+L",
        ToolTip          = "Clear"
    };

    public static readonly KeyGesture ClearCommandKeyGesture = ClearCommand.InputGestureAsKeyGesture;

    public static readonly RoutedUICommandEx PasteFoldersWithSubfoldersCommand = new()
    {
        Text             = "_Paste (append ** active file type mask to folders)",
        InputGestureText = "Ctrl+Alt+V"
    };

    public static readonly KeyGesture PasteFoldersWithSubfoldersCommandKeyGesture = PasteFoldersWithSubfoldersCommand.InputGestureAsKeyGesture;

    public static readonly RoutedUICommandEx PasteFromClipboardFoldersAndProceedCommand = new()
    {
        Text             = "Clear, paste (append * active file type mask to folders) a_nd proceed",
        InputGestureText = "Ctrl+Shift+V"
    };

    public static readonly KeyGesture PasteFromClipboardFoldersAndProceedCommandKeyGesture = PasteFromClipboardFoldersAndProceedCommand.InputGestureAsKeyGesture;

    public static readonly RoutedUICommandEx PasteFromClipboardFoldersWithSubfoldersAndProceedCommand = new()
    {
        Text             = "Clear, paste (append ** active file type mask to folders) a_nd proceed",
        InputGestureText = "Ctrl+Shift+Alt+V"
    };

    public static readonly KeyGesture PasteFromClipboardFoldersWithSubfoldersAndProceedCommandKeyGesture = PasteFromClipboardFoldersWithSubfoldersAndProceedCommand.InputGestureAsKeyGesture;

    public static readonly RoutedUICommandEx WrapFilesTextCommand = new()
    {
        Text             = "Word _wrap",
        InputGestureText = "Ctrl+W"
    };

    public static readonly KeyGesture WrapFilesTextCommandKeyGesture = WrapFilesTextCommand.InputGestureAsKeyGesture;

    public static readonly RoutedUICommandEx CtrlLeftClickCommand = new()
    {
        InputGestureText = "Ctrl+LeftClick"
    };

    public static readonly MouseGesture CtrlLeftClickCommandMouseGesture = CtrlLeftClickCommand.InputGestureAsMouseGesture;

    public static readonly RoutedUICommandEx BrowseCommand = new()
    {
        Text             = "_Browse",
        InputGestureText = "Ctrl+F",
        ToolTip          = $"Browse file or folder at the current line ({{0}}) or any line ({CtrlLeftClickCommand.InputGestureText})"
    };

    public static readonly KeyGesture BrowseCommandKeyGesture = BrowseCommand.InputGestureAsKeyGesture;

    public static readonly RoutedUICommandEx HelpCommand = new()
    {
        InputGestureText = "F1"
    };

    public static readonly KeyGesture HelpCommandKeyGesture = HelpCommand.InputGestureAsKeyGesture;

    public static readonly string ConnectionHelp = $"View context help on GitHub. {HelpCommand.InputGestureText} for driver help on GitHub";

#pragma warning disable S1075
    private const string BaseHelpUri             = "https://github.com/i2van/CsvLINQPadDriver#";
#pragma warning restore S1075
    private const string HelpUri                 = $"{BaseHelpUri}readme";
    public static readonly Uri FilesHelp         = new($"{BaseHelpUri}csv-files");
    public static readonly Uri FormatHelp        = new($"{BaseHelpUri}format");
    public static readonly Uri MemoryHelp        = new($"{BaseHelpUri}memory");
    public static readonly Uri GenerationHelp    = new($"{BaseHelpUri}generation");
    public static readonly Uri RelationsHelp     = new($"{BaseHelpUri}relations");

    // ReSharper restore StringLiteralTypo

    private static T IfWin10<T>(T ifTrue, T ifFalse) =>
        Environment.OSVersion.Version.Major >= 10 ? ifTrue : ifFalse;

    public static readonly string WrapText   = IfWin10("⭹", "Wrap");
    public static readonly string UnwrapText = IfWin10("⭲", "Unwrap");

    private static string GetTurnWrapText(bool on) =>
        $"Turn {(on ? "on" : "off")} word wrap ({WrapFilesTextCommand.InputGestureText})";

    public static readonly string TurnOnWrapText  = GetTurnWrapText(true);
    public static readonly string TurnOffWrapText = GetTurnWrapText(false);
}
