using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;

using CsvLINQPadDriver.Extensions;
using CsvLINQPadDriver.Wpf;
using CsvLINQPadDriver.Wpf.Extensions;

namespace CsvLINQPadDriver
{
    internal partial class ConnectionDialog
    {
        private const string HelpUri = "https://github.com/i2van/CsvLINQPadDriver/#readme";

        public const Visibility NetCoreOnlyVisibility =
#if NETCOREAPP
            Visibility.Visible;
#else
            Visibility.Collapsed;
#endif

        // ReSharper disable StringLiteralTypo
        public static readonly RoutedUICommandEx AddFilesCommand = new()
        {
            Text = "Add f_iles",
            InputGestureText = "Ctrl+O",
            ToolTip = "Add files"
        };

        public static readonly KeyGesture AddFilesCommandKeyGesture = AddFilesCommand.InputGestureAsKeyGesture;

        public static readonly RoutedUICommandEx AddFoldersCommand = new()
        {
            Text = "Add f_olders",
            InputGestureText = "Ctrl+Shift+O",
            ToolTip = "Add folders"
        };

        public static readonly KeyGesture AddFoldersCommandKeyGesture = AddFoldersCommand.InputGestureAsKeyGesture;

        public static readonly RoutedUICommandEx AddFoldersWithSubfoldersCommand = new()
        {
            Text = "Add folders with _sub-folders",
            InputGestureText = "Ctrl+Shift+Alt+O",
            ToolTip = "Add folders with sub-folders"
        };

        public static readonly KeyGesture AddFoldersWithSubfoldersCommandKeyGesture = AddFoldersWithSubfoldersCommand.InputGestureAsKeyGesture;

        public static readonly RoutedUICommandEx ClearCommand = new()
        {
            Text = "Clea_r",
            InputGestureText = "Ctrl+L",
            ToolTip = "Clear"
        };

        public static readonly KeyGesture ClearCommandKeyGesture = ClearCommand.InputGestureAsKeyGesture;

        public static readonly RoutedUICommandEx PasteFoldersWithSubfoldersCommand = new()
        {
            Text = "Past_e (append **.csv to folders)",
            InputGestureText = "Ctrl+Alt+V"
        };

        public static readonly KeyGesture PasteFoldersWithSubfoldersCommandKeyGesture = PasteFoldersWithSubfoldersCommand.InputGestureAsKeyGesture;

        public static readonly RoutedUICommandEx PasteFromClipboardFoldersAndProceedCommand = new()
        {
            Text = "Clear, paste (append *.csv to folders) a_nd proceed",
            InputGestureText = "Ctrl+Shift+V"
        };

        public static readonly KeyGesture PasteFromClipboardFoldersAndProceedCommandKeyGesture = PasteFromClipboardFoldersAndProceedCommand.InputGestureAsKeyGesture;

        public static readonly RoutedUICommandEx PasteFromClipboardFoldersWithSubfoldersAndProceedCommand = new()
        {
            Text = "Clear, paste (append **.csv to folders) an_d proceed",
            InputGestureText = "Ctrl+Shift+Alt+V"
        };

        public static readonly KeyGesture PasteFromClipboardFoldersWithSubfoldersAndProceedCommandKeyGesture = PasteFromClipboardFoldersWithSubfoldersAndProceedCommand.InputGestureAsKeyGesture;

        public static readonly RoutedUICommandEx WrapFilesTextCommand = new()
        {
            Text = "Word _wrap",
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
            Text = "_Browse",
            InputGestureText = "Ctrl+F",
            ToolTip = $"Browse file or folder at the current line ({{0}}) or any line ({CtrlLeftClickCommand.InputGestureText})"
        };

        public static readonly KeyGesture BrowseCommandKeyGesture = BrowseCommand.InputGestureAsKeyGesture;

        public static readonly RoutedUICommandEx HelpCommand = new()
        {
            InputGestureText = "F1"
        };

        public static readonly KeyGesture HelpCommandKeyGesture = HelpCommand.InputGestureAsKeyGesture;

        public static readonly string ConnectionHelp = $"CSV Files Connection help ({HelpCommand.InputGestureText} for driver help) on GitHub";
        // ReSharper restore StringLiteralTypo

        private static T IfWin10<T>(T ifTrue, T ifFalse) =>
            Environment.OSVersion.Version.Major >= 10 ? ifTrue : ifFalse;

        public static readonly string WrapText   = IfWin10("⭹", "Wrap");
        public static readonly string UnwrapText = IfWin10("⭲", "Unwrap");

        private static string GetTurnWrapText(bool on) =>
            $"Turn {(on ? "on" : "off")} word wrap ({WrapFilesTextCommand.InputGestureText})";

        public static readonly string TurnOnWrapText  = GetTurnWrapText(true);
        public static readonly string TurnOffWrapText = GetTurnWrapText(false);

        public static readonly string WildcardsToolTip = $"Type one file/folder per line. Wildcards ? and * are supported; {FileExtensions.DefaultRecursiveMask} searches in folder and its sub-folders";

        private bool _addFoldersWithSubfoldersDialogOpened;

        public ConnectionDialog(ICsvDataContextDriverProperties csvDataContextDriverProperties)
        {
            DataContext = csvDataContextDriverProperties;

            OverrideDependencyPropertiesMetadata();
            InitializeComponent();
            SetupControls();
            AddCommandManagerPreviewHandlers();

            static void OverrideDependencyPropertiesMetadata()
            {
                Array.ForEach(new[] { typeof(Control), typeof(Hyperlink) }, type => ToolTipService.ShowOnDisabledProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(true)));

                var showDurationPropertyType = typeof(FrameworkElement);
                var showDurationProperty = ToolTipService.ShowDurationProperty;
                var duration = showDurationProperty.GetMetadata(showDurationPropertyType);
                showDurationProperty.OverrideMetadata(showDurationPropertyType, new FrameworkPropertyMetadata((int)duration.DefaultValue * 125 / 100));
            }

            void AddCommandManagerPreviewHandlers()
            {
                CommandManager.AddPreviewExecutedHandler(FilesTextBox, FilesTextBox_OnPreviewExecuted);
                CommandManager.AddPreviewCanExecuteHandler(FilesTextBox, FilesTextBox_OnPreviewCanExecute);
            }

            void SetupControls() =>
                FilesTextBox.ToolTip = $"{FilesTextBox.ToolTip} or {char.ToLower(WildcardsToolTip[0])}{WildcardsToolTip[1..]}".Replace(". ", Environment.NewLine);
        }

        private ICsvDataContextDriverProperties TypedDataContext =>
            (ICsvDataContextDriverProperties) DataContext;

        protected override void OnClosed(EventArgs e)
        {
            CommandManager.RemovePreviewExecutedHandler(FilesTextBox, FilesTextBox_OnPreviewExecuted);
            CommandManager.RemovePreviewCanExecuteHandler(FilesTextBox, FilesTextBox_OnPreviewCanExecute);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (CanClose())
            {
                DialogResult = true;
            }

            bool CanClose()
            {
                var validateFilePaths = TypedDataContext.ValidateFilePaths;
                if (!validateFilePaths)
                {
                    return true;
                }

                var invalidFilePaths = GetInvalidFilePaths();
                if (!invalidFilePaths.Any())
                {
                    return true;
                }

                var instructionText = $"Invalid file {invalidFilePaths.Pluralize("path")}";

                var canClose =
                    this.ShowYesNoDialog(
                        $"{instructionText} found",
                        $"Would you like to correct {invalidFilePaths.Pluralize("it", "them")}?",
                        "Go back and correct. Only absolute and network paths are supported",
                        "Proceed as is",
                        instructionText.ToLowerInvariant(),
                        invalidFilePaths.JoinNewLine(),
                        ValidateFilePathsCheckBox.ReplaceHotKeyChar("&"),
                        ref validateFilePaths) == false;

                TypedDataContext.ValidateFilePaths = validateFilePaths;
                ValidateFilePathsCheckBox.UpdateTargetBinding(ToggleButton.IsCheckedProperty);

                if (!canClose)
                {
                    FilesTextBox.Focus();
                }

                return canClose;

                IReadOnlyCollection<string> GetInvalidFilePaths() =>
                    FilesTextBox.Text.GetFilesOnly().Where(file => !IsPathValid(file)).ToList();
            }
        }

        private void FilesTextBox_DragEnter(object sender, DragEventArgs e)
        {
            e.Handled =
                e.Data.GetDataPresent(DataFormats.FileDrop) ||
                e.Data.GetDataPresent(DataFormats.StringFormat);

            e.Effects = e.Handled
                ? IsDragAndDropInAddMode(e.KeyStates)
                    ? DragDropEffects.Copy
                    : DragDropEffects.Move
                : DragDropEffects.None;
        }

        private void FilesTextBox_DragDrop(object sender, DragEventArgs e)
        {
            if (!IsDragAndDropInAddMode(e.KeyStates))
            {
                FilesTextBox.Clear();
            }

            var enrichedFiles = GetEnrichedPathsFromUserInput(
                (e.Data.GetData(DataFormats.FileDrop, true) ?? e.Data.GetData(DataFormats.StringFormat))!,
                IsDragAndDropInFoldersWithSubfoldersMode() ^ _addFoldersWithSubfoldersDialogOpened);

            AppendFiles(enrichedFiles);

            e.Handled = true;

            bool IsDragAndDropInFoldersWithSubfoldersMode() =>
                e.KeyStates.HasFlag(DragDropKeyStates.AltKey);
        }

        private static bool CanExecutePastePreview(ICommand command) =>
            command == ApplicationCommands.Paste && CanExecutePaste();

        private static void FilesTextBox_OnPreviewCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (CanExecutePastePreview(e.Command))
            {
                e.CanExecute = true;
                e.Handled = true;
            }
        }

        private void FilesTextBox_OnPreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (CanExecutePastePreview(e.Command))
            {
                InsertFilesFromClipboard(false);
                e.Handled = true;
            }
        }

        private void PasteFoldersWithSubfoldersCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e) =>
            InsertFilesFromClipboard(true);

        private void InsertFilesFromClipboard(bool folderAndItsSubfolders)
        {
            var enrichedFilesText = GetFilesTextFromClipboard(folderAndItsSubfolders);

            FilesTextBox.SelectedText = enrichedFilesText;
            FilesTextBox.SelectionLength = 0;
            FilesTextBox.SelectionStart += enrichedFilesText.Length;
        }

        private string[] GetEnrichedPathsFromUserInput(object data, bool folderAndItsSubfolders)
        {
            var files = data as IEnumerable<string> ?? ((string) data).GetFilesOnly();

            return files.Select(GetEnrichedPath).ToArray();

            string GetEnrichedPath(string path)
            {
                var (isFile, enrichedPath) = path.DeduceIsFileOrFolder();
                return isFile
                    ? enrichedPath
                    : Path.Combine(enrichedPath, GetFolderFilesMask(folderAndItsSubfolders));
            }
        }

        private static bool IsDragAndDropInAddMode(DragDropKeyStates keyStates) =>
            keyStates.HasFlag(DragDropKeyStates.ControlKey);

        private void ConnectionDialog_OnLoaded(object sender, RoutedEventArgs e) =>
            MoveCaretToEnd();

        private void CommandBinding_OnCanAlwaysExecute(object sender, CanExecuteRoutedEventArgs e) =>
            e.CanExecute = true;

        private void PasteFromClipboardFoldersAndProceedCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e) =>
            PasteAndGo(false);

        private void PasteFromClipboardFoldersWithSubfoldersAndProceedCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e) =>
            PasteAndGo(true);

        private string GetFilesTextFromClipboard(bool folderAndItsSubfolders) =>
            GetEnrichedPathsFromUserInput(Clipboard.GetData(DataFormats.FileDrop) ?? Clipboard.GetText(), folderAndItsSubfolders)
                .Aggregate(new StringBuilder(), (result, file) => result.AppendLine(file))
                .ToString();

        private void PasteAndGo(bool folderAndItsSubfolders)
        {
            var enrichedFilesText = GetFilesTextFromClipboard(folderAndItsSubfolders);

            FilesTextBox.SelectAll();
            FilesTextBox.SelectedText = enrichedFilesText;
            FilesTextBox.SelectionLength = 0;
            MoveCaretToEnd();

            OkButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }

        private void ClipboardCommandBinding_OnCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
            e.CanExecute = CanExecutePaste();

        private static bool CanExecutePaste() =>
            Clipboard.ContainsText(TextDataFormat.Text) ||
            Clipboard.ContainsText(TextDataFormat.UnicodeText) ||
            Clipboard.ContainsFileDropList();

        private void HelpCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e) =>
            (e.OriginalSource switch
            {
                Hyperlink hyperlink => hyperlink.NavigateUri.OriginalString,
                _ => HelpUri
            }).ShellExecute();

        private void AddFilesCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var fileType = TypedDataContext.FileType;

            if (this.TryOpenFiles("Add Files", FileExtensions.Filter, fileType.GetExtension(), out var files, fileType.GetFilterIndex()))
            {
                AppendFiles(files);
            }
        }

        private void AddFoldersCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e) =>
            AddFolders(false);

        private void AddFoldersWithSubfoldersCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e) =>
            AddFolders(true);

        private void ClearCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e) =>
            FilesTextBox.Clear();

        private void ClearCommandBinding_OnCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
            e.CanExecute = FilesTextBox?.Text.Any() == true;

        private void WrapFilesTextCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            FilesTextBox.TextWrapping =
                FilesTextBox.TextWrapping == TextWrapping.Wrap
                    ? TextWrapping.NoWrap
                    : TextWrapping.Wrap;

            ScrollToActiveLine();
        }

        private void CtrlLeftClickCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SetCaretIndexFromMousePosition();

            BrowseCommand.Execute(null, FilesTextBox);

            void SetCaretIndexFromMousePosition()
            {
                var caretIndex = FilesTextBox.GetCharacterIndexFromPoint(Mouse.GetPosition(FilesTextBox), true);
                if (caretIndex >= 0)
                {
                    FilesTextBox.CaretIndex = caretIndex;
                }
            }
        }

        private void BrowseCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (!TryGetFullPathAtLineIncludingInlineComment(out var fullPath))
            {
                this.ShowWarning("Only absolute paths are supported");
                return;
            }

            ScrollToActiveLine();

            var (isFile, path) = fullPath.DeduceIsFileOrFolder(true);
            var shellResult = path.Explore(isFile);

            if (!shellResult)
            {
                this.ShowWarning(
                    "Browse failed for".JoinNewLine(
                    string.Empty,
                    path,
                    string.Empty,
                    shellResult));
            }
        }

        private void ScrollToActiveLine()
        {
            FilesTextBox.ScrollToLine(GetActiveLineIndex());

            int GetActiveLineIndex() =>
                FilesTextBox.GetLineIndexFromCharacterIndex(FilesTextBox.CaretIndex);
        }

        private void BrowseCommandBinding_OnCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
            e.CanExecute = TryGetFullPathAtLineIncludingInlineComment(out _);

        private static bool IsPathValid(string path) =>
#if NETCOREAPP
            Path.IsPathFullyQualified(path);
#else
            Path.IsPathRooted(path);
#endif

        private bool TryGetFullPathAtLineIncludingInlineComment(out string fullPath)
        {
            if (FilesTextBox is null)
            {
                fullPath = string.Empty;
                return false;
            }

            return TryGetLineAtCaret(out fullPath) && IsPathValid(fullPath);

            bool TryGetLineAtCaret(out string line) =>
                !string.IsNullOrWhiteSpace(line = FilesTextBox.GetLineTextAtCaretIndex().GetInlineCommentContent().Trim());
        }

        private void AddFolders(bool withSubfolders)
        {
            _addFoldersWithSubfoldersDialogOpened = withSubfolders;

            if (this.TryBrowseForFolders($"Add Folders{(withSubfolders ? " with Sub-folders" : string.Empty)}", out var folders))
            {
                var folderFilesMask = GetFolderFilesMask(withSubfolders);
                AppendFiles(folders.Select(folder => Path.Combine(folder, folderFilesMask)).ToArray());
            }

            _addFoldersWithSubfoldersDialogOpened = false;
        }

        private string GetFolderFilesMask(bool withSubfolders) =>
            TypedDataContext.FileType.GetMask(withSubfolders);

        private void AppendFiles(params string[] files)
        {
            AppendNewLine();

            FilesTextBox.AppendText(files.JoinNewLine());
            FilesTextBox.AppendText(Environment.NewLine);

            MoveCaretToEnd(true);

            void AppendNewLine()
            {
                if (HasFiles() && !Regex.IsMatch(FilesTextBox.Text, @"[\r\n]$"))
                {
                    FilesTextBox.AppendText(Environment.NewLine);
                }

                bool HasFiles() =>
                    !string.IsNullOrWhiteSpace(FilesTextBox?.Text);
            }
        }

        private void MoveCaretToEnd(bool scrollToEnd = false)
        {
            FilesTextBox.CaretIndex = FilesTextBox.Text.Length;
            if (scrollToEnd)
            {
                FilesTextBox.ScrollToEnd();
            }
        }

        private void UnstableOption_OnChecked(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }

            var checkBox = e.OriginalSource as CheckBox;

            if (((string?)checkBox?.Tag)?.ToBool() != true)
            {
                return;
            }

            checkBox.IsChecked = this.ShowYesNoDialog(
                checkBox.ReplaceHotKeyChar(),
                "This option might mangle CSV parsing. Would you like to use it anyway?",
                "I understand and want to use it",
                "Do not use",
                false) == true;
        }
    }
}
