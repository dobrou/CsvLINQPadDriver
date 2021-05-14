namespace CsvLINQPadDriver.Wpf
{
    internal static class DialogExtensions
    {
        public static bool TryOpenFile(this string title, string filter, string defaultExt, out string[] fileNames)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = title,
                Filter = filter,
                DefaultExt = defaultExt,
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = true,
                ValidateNames = true
            };

            var result = openFileDialog.ShowDialog() == true;

            fileNames = result
                ? openFileDialog.FileNames
                : new string[0];

            return result;
        }

        public static bool TryBrowseForFolder(this string description, out string folder)
        {
            var folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = description,
                UseDescriptionForTitle = true
            };

            var result = folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK;

            folder = result
                ? folderBrowserDialog.SelectedPath
                : string.Empty;

            return result;
        }
    }
}
