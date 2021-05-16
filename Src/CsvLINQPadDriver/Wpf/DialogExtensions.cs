using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Interop;

using Microsoft.WindowsAPICodePack.Dialogs;

namespace CsvLINQPadDriver.Wpf
{
    internal static class DialogExtensions
    {
        public static bool TryOpenFile(this string title, string filter, string defaultExt, out string[] fileNames, int filterIndex = 1)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = title,
                Filter = filter,
                DefaultExt = string.IsNullOrEmpty(defaultExt) ? string.Empty : $".{defaultExt.TrimStart('.')}",
                FilterIndex = filterIndex,
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
            using var folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog
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

        public static void ShowWarning(this Window owner, string text) =>
            MessageBox.Show(owner, AppendDot(text), owner.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);

        public static bool? ShowYesNoDialog(
            this Window owner,
            string instructionText,
            string text,
            string yesInstructions,
            string noInstructions,
            bool yesIsDefault = true
        )
        {
            var footerCheckBoxChecked = false;
            return owner.ShowYesNoDialog(instructionText, text, yesInstructions, noInstructions, null, null, null, ref footerCheckBoxChecked, yesIsDefault);
        }

        public static bool? ShowYesNoDialog(
            this Window owner,
            string instructionText,
            string text,
            string yesInstructions,
            string noInstructions,
            string? detailsLabel,
            string? detailsExpandedText,
            string? footerCheckBoxText,
            ref bool footerCheckBoxChecked,
            bool yesIsDefault = true
        )
        {
            var hasFooterCheckBox = footerCheckBoxText is not null;
            var hasDetailsLabel = detailsLabel is not null;

            using var taskDialog = new TaskDialog
            {
                OwnerWindowHandle = new WindowInteropHelper(owner).Handle,
                StartupLocation = TaskDialogStartupLocation.CenterOwner,
                Cancelable = true,
                
                Caption = owner.Title,
                InstructionText = instructionText,
                Text = AppendDot(text),
                Icon = TaskDialogStandardIcon.Warning,

                Controls =
                {
                    CreateTaskDialogCommandLink("&Yes", AppendDot(yesInstructions), TaskDialogResult.Yes, yesIsDefault),
                    CreateTaskDialogCommandLink("&No",  AppendDot(noInstructions),  TaskDialogResult.No,  !yesIsDefault)
                }
            };

            if (hasFooterCheckBox)
            {
                taskDialog.FooterCheckBoxText = footerCheckBoxText;
                taskDialog.FooterCheckBoxChecked = footerCheckBoxChecked;
            }

            if (hasDetailsLabel)
            {
                taskDialog.DetailsCollapsedLabel = $"S&how {detailsLabel}";
                taskDialog.DetailsExpandedLabel  = $"&Hide {detailsLabel}";
                taskDialog.DetailsExpandedText = detailsExpandedText;
            }

            bool? result = taskDialog.Show() switch
            {
                TaskDialogResult.Yes => true,
                TaskDialogResult.No => false,
                _ => null
            };

            footerCheckBoxChecked = taskDialog.FooterCheckBoxChecked == true;

            foreach (var taskDialogCommandLink in taskDialog.Controls.OfType<TaskDialogCommandLink>())
            {
                taskDialogCommandLink.Click -= TaskDialogCommandLinkClicked!;
            }

            return result;

            static TaskDialogCommandLink CreateTaskDialogCommandLink(string text, string instructions, TaskDialogResult taskDialogResult, bool isDefault = false)
            {
                var taskDialogCommandLink = new TaskDialogCommandLink(taskDialogResult.ToString(), text, instructions)
                {
                    Default = isDefault
                };

                taskDialogCommandLink.Click += TaskDialogCommandLinkClicked!;

                return taskDialogCommandLink;
            }

            static void TaskDialogCommandLinkClicked(object sender, EventArgs e)
            {
                var taskDialogCommandLink = (TaskDialogCommandLink)sender;
                ((TaskDialog)taskDialogCommandLink.HostingDialog).Close(Enum.Parse<TaskDialogResult>(taskDialogCommandLink.Name));
            }
        }

        private static string AppendDot(string str) =>
            Regex.IsMatch(str, @"\p{P}\s*$")
                ? str
                : str + ".";
    }
}
