using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;

using CsvLINQPadDriver.Extensions;
using CsvLINQPadDriver.Wpf;

namespace CsvLINQPadDriver
{
    public partial class ConnectionDialog
    {
        private const string HelpUri = "https://github.com/i2van/CsvLINQPadDriver/blob/master/README.md#csvlinqpaddriver-for-linqpad-6";

        private bool _addFolderAndItsSubfoldersDialogOpened;

        public static readonly RoutedUICommand CtrlLeftClickCommand = new();

        public ConnectionDialog(ICsvDataContextDriverProperties csvDataContextDriverProperties)
        {
            DataContext = csvDataContextDriverProperties;

            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) =>
            DialogResult = true;

        private void FilesTextBox_DragEnter(object sender, DragEventArgs e) =>
            e.Effects = (e.Handled = true) && e.Data.GetDataPresent(DataFormats.FileDrop)
                ? IsDragAndDropInAddMode(e.KeyStates)
                    ? DragDropEffects.Copy
                    : DragDropEffects.Move
                : DragDropEffects.None;

        private void FilesTextBox_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop, true) is not IEnumerable<string> files)
            {
                return;
            }

            if (!IsDragAndDropInAddMode(e.KeyStates))
            {
                FilesTextBox.Clear();
            }

            AppendFiles(files.Select(GetEnrichedPath).ToArray());

            string GetEnrichedPath(string path)
            {
                try
                {
                    // Add *.csv mask to dirs.
                    return Directory.Exists(path)
                        ? Path.Combine(path, GetFolderFilesMask(_addFolderAndItsSubfoldersDialogOpened))
                        : path;
                }
                catch (Exception exception)
                {
                    ShowWarning(string.Join(Environment.NewLine,
                                    $"Could not determine whether {path} is a file or folder due to error:",
                                    string.Empty,
                                    exception.Message,
                                    string.Empty,
                                    "Assuming a file."));
                    return path;
                }
            }
        }

        private static bool IsDragAndDropInAddMode(DragDropKeyStates keyStates) =>
            keyStates.HasFlag(DragDropKeyStates.ControlKey);

        private void ConnectionDialog_OnLoaded(object sender, RoutedEventArgs e) =>
            MoveCaretToEnd();

        private void CommandBinding_OnCanExecuteTrue(object sender, CanExecuteRoutedEventArgs e) =>
            e.CanExecute = true;

        private void ClearPasteClipboardAndProceedCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            FilesTextBox.SelectAll();
            PasteAndGo();
        }

        private void PasteClipboardAndProceedCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            AppendNewLine();
            MoveCaretToEnd();
            PasteAndGo();
        }

        private void PasteAndGo()
        {
            FilesTextBox.Paste();

            OkButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }

        private void ClipboardCommandBinding_OnCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
            e.CanExecute = Clipboard.ContainsText(TextDataFormat.Text) ||
                           Clipboard.ContainsText(TextDataFormat.UnicodeText);

        private void HelpCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e) =>
            (e.OriginalSource switch
            {
                Hyperlink hyperlink => hyperlink.NavigateUri.OriginalString,
                _ => HelpUri
            }).ShellExecute();

        private void AddFileCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if ("Add Files".TryOpenFile("CSV Files|*.csv|TSV Files|*.tsv|Text Files|*.txt|Log Files|*.log|All Files|*.*", "csv", out var fileName))
            {
                AppendFiles(fileName);
            }
        }

        private void AddFolderCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e) =>
            AddFolder(false);

        private void AddFolderAndItsSubfoldersCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e) =>
            AddFolder(true);

        private void ClearCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            FilesTextBox.Clear();
            FilesTextBox.Focus();
        }

        private void ClearCommandBinding_OnCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
            e.CanExecute = FilesTextBox?.Text.Any() == true;

        private void CtrlLeftClickCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (!ReferenceEquals(e.OriginalSource, FilesTextBox))
            {
                return;
            }

            SetCaretIndexFromMousePosition();

            ApplicationCommands.Find.Execute(null, FilesTextBox);

            void SetCaretIndexFromMousePosition()
            {
                var caretIndex = FilesTextBox.GetCharacterIndexFromPoint(Mouse.GetPosition(FilesTextBox), true);
                if (caretIndex >= 0)
                {
                    FilesTextBox.CaretIndex = caretIndex;
                }
            }
        }

        private void BrowseFileOrFolderCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (!TryGetFullPathAtLine(out var fullPath))
            {
                ShowWarning("Only absolute paths are supported.");
                return;
            }

            var (isFile, path) = fullPath!.DeduceFileOrFolder();
            var shellResult = path.Explore(isFile);

            if (!shellResult)
            {
                ShowWarning(shellResult);
            }
        }

        private void ShowWarning(string text) =>
            MessageBox.Show(this, text, Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);

        private void BrowseFileOrFolderCommandBinding_OnCanExecuteTrue(object sender, CanExecuteRoutedEventArgs e) =>
            e.CanExecute = TryGetFullPathAtLine(out _);

        private bool TryGetFullPathAtLine(out string? fullPath)
        {
            return TryGetLineAtCaret(out fullPath) && Path.IsPathFullyQualified(fullPath!);

            bool TryGetLineAtCaret(out string? line) =>
                !string.IsNullOrWhiteSpace(line = FilesTextBox?.GetLineText(FilesTextBox.GetLineIndexFromCharacterIndex(FilesTextBox.CaretIndex)).Trim());
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

        private static string GetFolderFilesMask(bool withSubfolders) =>
            $"{(withSubfolders ? "**" : "*")}.csv";

        private void AppendFiles(params string[] files)
        {
            AppendNewLine();

            FilesTextBox.AppendText(string.Join(Environment.NewLine, files));
            FilesTextBox.AppendText(Environment.NewLine);

            MoveCaretToEnd(true);
        }

        private void AppendNewLine()
        {
            if (HasFiles() && !Regex.IsMatch(FilesTextBox.Text, @"[\r\n]$"))
            {
                FilesTextBox.AppendText(Environment.NewLine);
            }

            bool HasFiles() =>
                !string.IsNullOrWhiteSpace(FilesTextBox?.Text);
        }

        private void MoveCaretToEnd(bool scrollToEnd = false)
        {
            FilesTextBox.CaretIndex = FilesTextBox.Text.Length;
            if (scrollToEnd)
            {
                FilesTextBox.ScrollToEnd();
            }
        }
    }
}
