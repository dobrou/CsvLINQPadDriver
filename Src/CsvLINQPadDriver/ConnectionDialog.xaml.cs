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

        public const string ConfirmCheck = "check";
        public const string ConfirmUncheck = "uncheck";

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

        public static readonly RoutedUICommandEx DeleteCommand = new()
        {
            Text = "_Delete",
            InputGestureText = "Del"
        };

        public static readonly RoutedUICommandEx SelectAllCommand = new()
        {
            Text = "Select _all",
            InputGestureText = "Ctrl+A"
        };

        public static readonly RoutedUICommandEx ClearCommand = new()
        {
            Text = "C_lear",
            InputGestureText = "Ctrl+L",
            ToolTip = "Clear"
        };

        public static readonly KeyGesture ClearCommandKeyGesture = ClearCommand.InputGestureAsKeyGesture;

        public static readonly RoutedUICommandEx PasteFoldersWithSubfoldersCommand = new()
        {
            Text = "_Paste (append **.csv to folders)",
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
            Text = "Clear, paste (append **.csv to folders) a_nd proceed",
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

        private object? _originalFilesTextBoxToolTip;
        private bool _addFoldersWithSubfoldersDialogOpened;
        private bool _skipConfirmOption;

        private ICsvDataContextDriverProperties TypedDataContext =>
            (ICsvDataContextDriverProperties)DataContext;

        public ConnectionDialog(ICsvDataContextDriverProperties csvDataContextDriverProperties)
        {
            DataContext = csvDataContextDriverProperties;

            OverrideDependencyPropertiesMetadata();
            InitializeComponent();
            UpdateInstructions();
            AddCommandManagerPreviewHandlers();

            static void OverrideDependencyPropertiesMetadata()
            {
                Array.ForEach(new[] { typeof(Control), typeof(Hyperlink) }, static type => ToolTipService.ShowOnDisabledProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(true)));

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
        }

        protected override void OnClosed(EventArgs e)
        {
            CommandManager.RemovePreviewExecutedHandler(FilesTextBox, FilesTextBox_OnPreviewExecuted);
            CommandManager.RemovePreviewCanExecuteHandler(FilesTextBox, FilesTextBox_OnPreviewCanExecute);
        }

        private void UpdateInstructions()
        {
            if (!IsInitialized)
            {
                return;
            }

            const string inlineComment = FileExtensions.InlineComment;

            var fileType = TypedDataContext.FileType;
            var mask = fileType.GetMask();
            var recursiveMask = fileType.GetMask(true);

            var wildcardsToolTip = $"Type one file/folder per line. Wildcards ? and * are supported; {recursiveMask} searches in folder and its sub-folders";

            FilesInstructionsTextBox.Text = GetInstructions().Select(static str => $"{inlineComment} {str}.").JoinNewLine();
            FilesTextBox.ToolTip = $"{_originalFilesTextBoxToolTip ??= FilesTextBox.ToolTip} or {char.ToLower(wildcardsToolTip[0])}{wildcardsToolTip[1..]}".Replace(". ", Environment.NewLine);

            IEnumerable<string> GetInstructions()
            {
                yield return "Drag&drop here (from add files/folder dialogs as well). Ctrl adds files. Alt toggles * and ** masks";
                yield return wildcardsToolTip;
                yield return $"{((KeyGesture)ApplicationCommands.Paste.InputGestures[0]).DisplayString} ({PasteFoldersWithSubfoldersCommand.InputGestureText}) pastes from clipboard, appends {mask} ({recursiveMask}) to folders";
                yield return $"{PasteFromClipboardFoldersAndProceedCommand.InputGestureText} ({PasteFromClipboardFoldersWithSubfoldersAndProceedCommand.InputGestureText}) clears, pastes from clipboard, appends {mask} ({recursiveMask}) to folders and proceeds";
                yield return $"Use '{inlineComment}' to comment line";
            }
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

                try
                {
                    _skipConfirmOption = true;

                    TypedDataContext.ValidateFilePaths = validateFilePaths;
                    ValidateFilePathsCheckBox.UpdateTargetBinding(ToggleButton.IsCheckedProperty);
                }
                finally
                {
                    _skipConfirmOption = false;
                }

                if (!canClose)
                {
                    FilesTextBox.Focus();
                }

                return canClose;

                IReadOnlyCollection<string> GetInvalidFilePaths() =>
                    FilesTextBox.Text.GetFilesOnly().Where(static file => !IsPathValid(file)).ToList();
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

        private void InsertFilesFromClipboard(bool foldersWithSubfolders)
        {
            var enrichedFilesText = GetFilesTextFromClipboard(foldersWithSubfolders);

            FilesTextBox.SelectedText = enrichedFilesText;
            FilesTextBox.SelectionLength = 0;
            FilesTextBox.SelectionStart += enrichedFilesText.Length;
        }

        private string[] GetEnrichedPathsFromUserInput(object data, bool foldersWithSubfolders)
        {
            var files = data as IEnumerable<string> ?? ((string) data).GetFilesOnly();

            return files.Select(GetEnrichedPath).ToArray();

            string GetEnrichedPath(string path)
            {
                var (isFile, enrichedPath) = path.DeduceIsFileOrFolder();
                return isFile
                    ? enrichedPath
                    : Path.Combine(enrichedPath, GetFolderFilesMask(foldersWithSubfolders));
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

        private string GetFilesTextFromClipboard(bool foldersWithSubfolders) =>
            GetEnrichedPathsFromUserInput(Clipboard.GetData(DataFormats.FileDrop) ?? Clipboard.GetText(), foldersWithSubfolders)
                .Aggregate(new StringBuilder(), static (result, file) => result.AppendLine(file))
                .ToString();

        private void PasteAndGo(bool foldersWithSubfolders)
        {
            var enrichedFilesText = GetFilesTextFromClipboard(foldersWithSubfolders);

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
                _                   => HelpUri
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

        private void SelectAllCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e) =>
            ApplicationCommands.SelectAll.Execute(null, FilesTextBox);

        private void SelectAllCommandBinding_OnCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
            e.CanExecute = FilesTextBox.SelectionLength < FilesTextBox.Text.Length;

        private void DeleteCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e) =>
            EditingCommands.Delete.Execute(null, FilesTextBox);

        private void DeleteCommandBinding_OnCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
            e.CanExecute = FilesTextBox.SelectionLength != 0 || FilesTextBox.CaretIndex < FilesTextBox.Text.Length;

        private void ClearCommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            FilesTextBox.Clear();
            FilesTextBox.Focus();
        }

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

        private void FileTypeComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e) =>
            UpdateInstructions();

        private void ConfirmOption_OnChecked(object sender, RoutedEventArgs e) =>
            ConfirmOption(e);

        private void ConfirmOption_OnUnchecked(object sender, RoutedEventArgs e) =>
            ConfirmOption(e, true);

        private void ConfirmOption(RoutedEventArgs e, bool isUnchecked = false)
        {
            if (_skipConfirmOption ||
                !IsLoaded ||
                e.OriginalSource is not CheckBox { Tag: string tag } checkBox ||
                e.RoutedEvent != (tag == ConfirmCheck ? ToggleButton.CheckedEvent : ToggleButton.UncheckedEvent))
            {
                return;
            }

            checkBox.IsChecked =
                isUnchecked ^ this.ShowYesNoDialog(
                    checkBox.ReplaceHotKeyChar(),
                    $"This option might mangle CSV parsing if {tag}ed. Would you like to {tag} it anyway?",
                    $"I understand and want to {tag} it",
                    $"Do not {tag} it",
                    false)
                ?? isUnchecked;
        }
    }
}
