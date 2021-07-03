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

        public static readonly RoutedUICommand AddFilesCommand = new();
        public static readonly RoutedUICommand AddFolderCommand = new();
        public static readonly RoutedUICommand AddFolderAndItsSubfoldersCommand = new();
        public static readonly RoutedUICommand PasteFromClipboardForFolderAndItsSubfoldersAndProceedCommand = new();
        public static readonly RoutedUICommand PasteFromClipboardForFolderAndProceedCommand = new();
        public static readonly RoutedUICommand ClearCommand = new();
        public static readonly RoutedUICommand HelpCommand = new();
        public static readonly RoutedUICommand BrowseCommand = new();
        public static readonly RoutedUICommand CtrlLeftClickCommand = new();
        public static readonly RoutedUICommand PasteWithFolderAndItsSubfoldersCommand = new();
        public static readonly RoutedUICommand WrapFilesTextCommand = new();

        public static readonly string WildcardsToolTip = $"Type one file/folder per line. Wildcards ? and * are supported; {FileExtensions.DefaultRecursiveMask} searches in folder and its sub-folders";

        private bool _addFolderAndItsSubfoldersDialogOpened;

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
                IsDragAndDropInFolderAndItsSubfoldersMode() ^ _addFolderAndItsSubfoldersDialogOpened);

            AppendFiles(enrichedFiles);

            e.Handled = true;

            bool IsDragAndDropInFolderAndItsSubfoldersMode() =>
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

        private void PasteWithFolderAndItsSubfoldersCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e) =>
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

        private void PasteFromClipboardForFolderAndProceedCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e) =>
            PasteAndGo(false);

        private void PasteFromClipboardForFolderAndItsSubfoldersAndProceedCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e) =>
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

            if ("Add Files".TryOpenFile(FileExtensions.Filter, fileType.GetExtension(), out var fileName, fileType.GetFilterIndex()))
            {
                AppendFiles(fileName);
            }
        }

        private void AddFolderCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e) =>
            AddFolder(false);

        private void AddFolderAndItsSubfoldersCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e) =>
            AddFolder(true);

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

        private void AddFolder(bool withSubfolders)
        {
            _addFolderAndItsSubfoldersDialogOpened = withSubfolders;

            if ($"Add Folder{(withSubfolders ? " and Its Sub-folders" : string.Empty)}".TryBrowseForFolder(out var folder))
            {
                AppendFiles(Path.Combine(folder, GetFolderFilesMask(withSubfolders)));
            }

            _addFolderAndItsSubfoldersDialogOpened = false;
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
