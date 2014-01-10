using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Path = System.Windows.Shapes.Path;

namespace CsvLINQPadFileOpen
{
    /// <summary>
    /// Interaction logic for FileDialog.xaml
    /// </summary>
    public partial class FileSelectDialog : Window
    {
        public string Files { get; set; }

        public FileSelectDialog(string files = null)
        {
            this.Files = files;
            DataContext = this;
            InitializeComponent();
        }

        void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void TextBox_DragEnter(object sender, DragEventArgs e)
        {
            e.Handled = true;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey))
                {
                    e.Effects = DragDropEffects.Copy;
                }
                else
                {
                    e.Effects = DragDropEffects.Link;
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void TextBox_DragDrop(object sender, DragEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            IEnumerable<string> files = e.Data.GetData(DataFormats.FileDrop, true) as string[];

            if (textBox != null && files != null)
            {
                //add *.csv mask to dirs
                files = files.Select(f => Directory.Exists(f) ? System.IO.Path.Combine(f, "*.csv") : f);

                //if ctrl, add files to textbox, instead of replacing whole text
                if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey))
                    files = new[] { textBox.Text }.Concat(files);

                textBox.Text = string.Join("\n", files);

                var binding = textBox.GetBindingExpression(TextBox.TextProperty);
                if (binding != null)
                {
                    binding.UpdateSource();
                }
            }
        }
    }
}
