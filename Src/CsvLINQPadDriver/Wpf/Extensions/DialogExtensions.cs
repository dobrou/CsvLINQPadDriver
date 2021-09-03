using System;
using System.Linq;
using System.Windows;
using System.Windows.Interop;

using Microsoft.WindowsAPICodePack.Dialogs;

using CsvLINQPadDriver.Extensions;

using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace CsvLINQPadDriver.Wpf.Extensions
{
    internal static class DialogExtensions
    {
        public static bool TryOpenFiles(this Window owner, string title, string filter, string defaultExt, out string[] files, int filterIndex = 1)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = title,
                Multiselect = true,
                ValidateNames = true,
                CheckPathExists = true,
                CheckFileExists = true,
                DefaultExt = string.IsNullOrEmpty(defaultExt) ? string.Empty : $".{defaultExt.TrimStart('.')}",
                Filter = filter,
                FilterIndex = filterIndex,
                AddExtension = true
            };

            var result = openFileDialog.ShowDialog(owner) == true;

            files = result
                        ? openFileDialog.FileNames
                        : Array.Empty<string>();

            return result;
        }

        public static bool TryBrowseForFolders(this Window owner, string title, out string[] folders)
        {
            using var commonOpenFileDialog = new CommonOpenFileDialog
            {
                Title = title,
                IsFolderPicker = true,
                Multiselect = true,
                EnsurePathExists = true,
                EnsureValidNames = true
            };

            var result = commonOpenFileDialog.ShowDialog(owner) == CommonFileDialogResult.Ok;

            folders = result
                ? commonOpenFileDialog.FileNames.ToArray()
                : Array.Empty<string>();

            return result;
        }

        public static void ShowWarning(this Window owner, string text) =>
            MessageBox.Show(owner, text.AppendDot(), owner.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);

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
                Text = text.AppendDot(),
                Icon = TaskDialogStandardIcon.Warning,

                Controls =
                {
                    CreateTaskDialogCommandLink("&Yes", yesInstructions.AppendDot(), TaskDialogResult.Yes, yesIsDefault),
                    CreateTaskDialogCommandLink("&No",  noInstructions.AppendDot(),  TaskDialogResult.No,  !yesIsDefault)
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
                TaskDialogResult.No  => false,
                _                    => null
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
                ((TaskDialog)taskDialogCommandLink.HostingDialog).Close(
#if NETCOREAPP
                    Enum.Parse<TaskDialogResult>(
#else
                    (TaskDialogResult)Enum.Parse(typeof(TaskDialogResult), 
#endif
                        taskDialogCommandLink.Name));
            }
        }
    }
}
