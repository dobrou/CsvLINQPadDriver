using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;

using CsvLINQPadDriver.Helpers;

namespace CsvLINQPadDriver
{
    public partial class ConnectionDialog
    {
        public ConnectionDialog(ICsvDataContextDriverProperties csvDataContextDriverProperties)
        {
            DataContext = csvDataContextDriverProperties;

            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) =>
            DialogResult = true;

        private void FilesTextBox_DragEnter(object sender, DragEventArgs e)
        {
            e.Handled = true;

            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
                ? e.KeyStates.HasFlag(DragDropKeyStates.ControlKey)
                    ? DragDropEffects.Copy
                    : DragDropEffects.Link
                : DragDropEffects.None;
        }

        private void FilesTextBox_DragDrop(object sender, DragEventArgs e)
        {
            var textBox = (TextBox)sender;

            if (!(e.Data.GetData(DataFormats.FileDrop, true) is IEnumerable<string> files))
            {
                return;
            }

            // Add *.csv mask to dirs.
            files = files.Select(path => Directory.Exists(path) ? Path.Combine(path, "*.csv") : path);

            // If Ctrl is pressed then add files to text box instead of replacing whole text.
            if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey))
            {
                files = new[] { textBox.Text }.Concat(files);
            }

            textBox.Text = string.Join(Environment.NewLine, files);

            textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        }

        private void ConnectionDialog_OnLoaded(object sender, RoutedEventArgs e)
        {
            var maskIndex = FilesTextBox.Text.IndexOf(FileUtils.GetDefaultDrive(), StringComparison.InvariantCultureIgnoreCase);
            if (maskIndex >= 0)
            {
                FilesTextBox.SelectionStart = maskIndex;
                FilesTextBox.SelectionLength = FilesTextBox.Text.Length - maskIndex;
            }
        }

        private void PasteAndGoCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            FilesTextBox.SelectAll();
            FilesTextBox.Paste();

            OkButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }

        private void PasteAndGoCommandBinding_OnCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
            e.CanExecute = Clipboard.ContainsText(TextDataFormat.Text) ||
                           Clipboard.ContainsText(TextDataFormat.UnicodeText);

        private void Help_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            using var process = Process.Start(new ProcessStartInfo(((Hyperlink) e.OriginalSource).NavigateUri.OriginalString) { UseShellExecute = true });
        }

        private void Help_OnCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
            e.CanExecute = true;
    }
}
