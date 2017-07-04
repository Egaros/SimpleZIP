﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using SimpleZIP_UI.Application.Compression;
using SimpleZIP_UI.Application.Model;
using SimpleZIP_UI.Application.Util;

namespace SimpleZIP_UI.Presentation.View
{
    public sealed partial class CompressionSummaryPage : Page, IDisposable
    {
        /// <summary>
        /// The aggregated page control instance.
        /// </summary>
        private readonly CompressionSummaryPageControl _control;

        /// <summary>
        /// A list of selected files for compression.
        /// </summary>
        private IReadOnlyList<StorageFile> _selectedFiles;

        /// <summary>
        /// Maps combo box items to file types for archives.
        /// </summary>
        private static readonly Dictionary<ComboBoxItem, string> FileTypesComboBoxItems;

        static CompressionSummaryPage()
        {
            FileTypesComboBoxItems = new Dictionary<ComboBoxItem, string>();
        }

        public CompressionSummaryPage()
        {
            InitializeComponent();
            ArchiveNameTextBox.Focus(FocusState.Programmatic);

            // populate combo box items
            ArchiveTypeComboBox.Items.Add(CreateItemForComboBox("ZIP (.zip)", ".zip"));
            ArchiveTypeComboBox.Items.Add(CreateItemForComboBox("GZIP (.gzip)", ".gzip"));
            ArchiveTypeComboBox.Items.Add(CreateItemForComboBox("TAR (.tar)", ".tar"));
            ArchiveTypeComboBox.Items.Add(CreateItemForComboBox("TAR+GZIP (.tgz)", ".tgz"));
            ArchiveTypeComboBox.Items.Add(CreateItemForComboBox("TAR+BZIP2 (.tbz2)", ".tbz2"));
            ArchiveTypeComboBox.Items.Add(CreateItemForComboBox("TAR+LZIP (.tlz)", ".tlz"));
            ArchiveTypeComboBox.SelectedIndex = 0; // selected index on page launch

            _control = new CompressionSummaryPageControl(this);
        }

        private static ComboBoxItem CreateItemForComboBox(string content, string fileType)
        {
            var item = new ComboBoxItem { Content = content };
            FileTypesComboBoxItems.Add(item, fileType);
            return item;
        }

        /// <summary>
        /// Invoked when the abort button has been tapped.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Arguments that may have been passed.</param>
        private void AbortButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            AbortButtonToolTip.IsOpen = true;
            _control.AbortButtonAction();
        }

        /// <summary>
        /// Invoked when the start button has been tapped.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Arguments that may have been passed.</param>
        private async void StartButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            var selectedItem = (ComboBoxItem)ArchiveTypeComboBox.SelectedItem;
            var archiveName = ArchiveNameTextBox.Text;

            if (archiveName.Length > 0 && !archiveName.ContainsIllegalChars())
            {
                // set the algorithm by archive file type
                FileTypesComboBoxItems.TryGetValue(selectedItem, out string archiveType);
                BaseControl.AlgorithmFileTypes.TryGetValue(archiveType, out BaseControl.Algorithm value);

                archiveName += archiveType;
                var result = await InitOperation(value, archiveName);

                // move focus to avoid accidental focus event on text block
                FocusManager.TryMoveFocus(FocusNavigationDirection.Next);

                _control.CreateResultDialog(result).ShowAsync().AsTask().Forget();

                Frame.Navigate(typeof(MainPage));
            }
        }


        /// <summary>
        /// Invoked when the panel containing the output path has been tapped.
        /// As a result, the user can pick an output folder for the archive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args">Arguments that may have been passed.</param>
        private void OutputPathPanel_Tap(object sender, TappedRoutedEventArgs args)
        {
            SetOutputPath();
        }

        /// <summary>
        /// Invoked when output path text block got focus.
        /// As a result, the user can pick an output folder for the archive.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Arguments that may have been passed.</param>
        private void OutputPathTextBlock_GotFocus(object sender, RoutedEventArgs args)
        {
            SetOutputPath();
        }

        /// <summary>
        /// Sets the output path and enables the start button if output path is valid.
        /// </summary>
        private async void SetOutputPath()
        {
            if (ProgressRing.IsActive) return;
            var text = OutputPathTextBlock.Text = await _control.PickOutputPath();
            StartButton.IsEnabled = !string.IsNullOrEmpty(text);
            FocusManager.TryMoveFocus(FocusNavigationDirection.Next);
        }

        /// <summary>
        /// Invoked when combo box for choosing the archive type has been closed.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="e">The event object.</param>
        private void ArchiveTypeComboBox_DropDownClosed(object sender, object e)
        {
            if (_selectedFiles.Count <= 1) return;
            var selectedItem = (ComboBoxItem)ArchiveTypeComboBox.SelectedItem;
            var archiveType = selectedItem?.Content?.ToString();

            if (archiveType != null && archiveType.Contains("gzip"))
            {
                ArchiveTypeToolTip.Content = "GZIP only allows to compress a single file.\r\n" +
                                             "Thus, each file will be put into a separate archive.";
                ArchiveTypeToolTip.IsOpen = true;
            }
        }

        /// <summary>
        /// Invoked when the text of the archive name input has beend modified.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Arguments that may have been passed.</param>
        private void ArchiveNameTextBox_TextChanged(object sender, TextChangedEventArgs args)
        {
            var fileName = ArchiveNameTextBox.Text;

            if (fileName.Length < 1) // reset if empty
            {
                ArchiveNameTextBox.Text = "myArchive";
            }
            else if (fileName.ContainsIllegalChars()) // check for illegal characters in file name
            {
                var content = "These characters are not allowed:\n" +
                    string.Join(" ", FileUtils.IllegalChars);
                ArchiveNameToolTip.Content = content;
                ArchiveNameToolTip.IsOpen = true;
            }
            else
            {
                ArchiveNameToolTip.IsOpen = false;
            }
        }

        /// <summary>
        /// Invoked when any tooltip has been opened.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Arguments that may have been passed.</param>
        private void ToolTip_Opened(object sender, RoutedEventArgs args)
        {
            var toolTip = (ToolTip)sender;

            // use timer to close tooltip after 8 seconds
            var timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 8) };
            timer.Tick += (s, evt) =>
            {
                toolTip.IsOpen = false;
                timer.Stop();
            };
            timer.Start();
        }

        /// <summary>
        /// Initializes the archiving operation and waits for the result.
        /// </summary>
        /// <param name="key">The type of the archive.</param>
        /// <param name="archiveName">The name of the archive.</param>
        private async Task<Result> InitOperation(BaseControl.Algorithm key, string archiveName)
        {
            SetOperationActive(true);
            var archiveInfo = new ArchiveInfo(OperationMode.Compress)
            {
                SelectedFiles = _selectedFiles,
                ArchiveName = archiveName,
                Algorithm = key
            };
            return await _control.StartButtonAction(archiveInfo);
        }

        /// <summary>
        /// Sets the archiving operation as active. This results in the UI being busy.
        /// </summary>
        /// <param name="isActive">True to set operation as active, false to set it as inactive.</param>
        private void SetOperationActive(bool isActive)
        {
            if (isActive)
            {
                ProgressRing.IsActive = true;
                ProgressRing.Visibility = Visibility.Visible;
                StartButton.IsEnabled = false;
                OutputPathTextBlock.IsEnabled = false;
                ArchiveNameTextBox.IsEnabled = false;
                ArchiveTypeComboBox.IsEnabled = false;
            }
            else
            {
                ProgressRing.IsActive = false;
                ProgressRing.Visibility = Visibility.Collapsed;
                StartButton.IsEnabled = true;
                OutputPathTextBlock.IsEnabled = true;
                ArchiveNameTextBox.IsEnabled = true;
                ArchiveTypeComboBox.IsEnabled = true;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            _selectedFiles = args.Parameter as IReadOnlyList<StorageFile>;

            if (_selectedFiles == null) return;

            foreach (var file in _selectedFiles) // populate list
            {
                ItemsListBox.Items?.Add(new TextBlock { Text = file.Name });
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            e.Cancel = _control.Operation?.IsRunning ?? false;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs args)
        {
            SetOperationActive(false);
            AbortButtonToolTip.IsOpen = false;
            ArchiveTypeToolTip.IsOpen = false;
        }

        public void Dispose()
        {
            _control.Dispose();
        }
    }
}
