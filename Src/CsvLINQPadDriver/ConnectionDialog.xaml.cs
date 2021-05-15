using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;

using CsvLINQPadDriver.Helpers;
using CsvLINQPadDriver.Wpf;

namespace CsvLINQPadDriver
{
    public partial class ConnectionDialog
    {
        private const string HelpUri = "https://github.com/i2van/CsvLINQPadDriver/blob/master/README.md#csvlinqpaddriver-for-linqpad-6";

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

            // Add *.csv mask to dirs.
            files = files.Select(path => Directory.Exists(path) ? Path.Combine(path, "*.csv") : path);

            if (!IsDragAndDropInAddMode(e.KeyStates))
            {
                FilesTextBox.Clear();
            }

            AppendFiles(files.ToArray());
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

        private void ClearCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e) =>
            FilesTextBox.Clear();

        private void ClearCommandBinding_OnCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
            e.CanExecute = FilesTextBox?.Text.Any() == true;

        private void BrowseFileOrFolderCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (TryGetLineAtCaret(out var line))
            {
                var (isFile, path) = line!.DeduceFileOrFolder();
                path.Explore(isFile);
            }
        }

        private void BrowseFileOrFolderCommandBinding_OnCanExecuteTrue(object sender, CanExecuteRoutedEventArgs e) =>
            e.CanExecute = TryGetLineAtCaret(out var line) &&
                           Path.IsPathFullyQualified(line!);

        private bool TryGetLineAtCaret(out string? line) =>
            !string.IsNullOrWhiteSpace(line = FilesTextBox?.GetLineText(FilesTextBox.GetLineIndexFromCharacterIndex(FilesTextBox.CaretIndex)).Trim());

        private void AddFolder(bool withSubfolders)
        {
            if ($"Add Folder{(withSubfolders ? " and Its Sub-folders" : string.Empty)}".TryBrowseForFolder(out var folder))
            {
                AppendFiles(Path.Combine(folder, $"{(withSubfolders ? "**" : "*")}.csv"));
            }
        }

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
