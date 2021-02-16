using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace CsvLINQPadDriver
{
    public partial class ConnectionDialog
    {
        public ConnectionDialog(ICsvDataContextDriverProperties properties)
        {
            DataContext = properties;
            Background = SystemColors.ControlBrush;
            InitializeComponent ();
        }

        void btnOK_Click (object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void TextBox_DragEnter(object sender, DragEventArgs e)
        {
            e.Handled = true;
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
                ? e.KeyStates.HasFlag(DragDropKeyStates.ControlKey)
                    ? DragDropEffects.Copy
                    : DragDropEffects.Link
                : DragDropEffects.None;
        }

        private void TextBox_DragDrop(object sender, DragEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            IEnumerable<string> files = e.Data.GetData(DataFormats.FileDrop, true) as string[];

            if (textBox != null && files != null)
            {
                //add *.csv mask to dirs
                files = files.Select(f => Directory.Exists(f) ? Path.Combine(f, "*.csv") : f);

                //if ctrl, add files to text box, instead of replacing whole text
                if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey))
                    files = new[] {textBox.Text}.Concat(files);
                    
                textBox.Text = string.Join("\n", files);
                
                var binding = textBox.GetBindingExpression(TextBox.TextProperty);
                binding?.UpdateSource();
            }
        }

        private void ConnectionDialog_OnLoaded(object sender, RoutedEventArgs e)
        {
            var maskIndex = txtFiles.Text.IndexOf(@"c:\", StringComparison.InvariantCultureIgnoreCase);
            if (maskIndex >= 0)
            {
                txtFiles.SelectionStart = maskIndex;
                txtFiles.SelectionLength = txtFiles.Text.Length - maskIndex;
            }
        }

        private void PasteAndGoCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            txtFiles.SelectAll();
            txtFiles.Paste();

            btnOK.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }

        private void PasteAndGoCommandBinding_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Clipboard.ContainsText(TextDataFormat.Text) || Clipboard.ContainsText(TextDataFormat.UnicodeText);
        }
    }
}
