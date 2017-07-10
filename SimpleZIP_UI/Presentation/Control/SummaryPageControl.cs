﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System.Display;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Application.Compression.Model;
using SimpleZIP_UI.Application.Compression.Operation;
using SimpleZIP_UI.Application.Util;
using SimpleZIP_UI.Presentation.Factory;

namespace SimpleZIP_UI.Presentation.Control
{
    internal abstract class SummaryPageControl<T> : BaseControl, IDisposable where T : OperationInfo
    {
        /// <summary>
        /// Specifies the threshold for the total file size 
        /// after which a notification will be displayed.
        /// </summary>
        private const ulong FileSizeWarningThreshold = 20971520; // 20 megabytes

        /// <summary>
        /// True if a cancel request has been made.
        /// </summary>
        public bool IsCancelRequest { get; protected set; }

        /// <summary>
        /// Reference to the currently active archiving operation.
        /// </summary>
        public ArchivingOperation<T> Operation;

        /// <summary>
        /// Where the archive or its content will be saved to.
        /// </summary>
        public StorageFolder OutputFolder { get; protected set; }

        internal SummaryPageControl(Page parent) : base(parent)
        {
            DisplayRequest = new DisplayRequest();
        }

        /// <summary>
        /// Performs an action when the start button has been tapped.
        /// </summary>
        /// <param name="operationInfos">The amount of operations to be performed.</param>
        /// <returns>True on success, false otherwise.</returns>
        internal async Task<Result> StartButtonAction(params T[] operationInfos)
        {
            using (Operation = GetArchivingOperation())
            {
                Result result;
                var startTime = DateTime.Now;

                try
                {
                    InitOperation(operationInfos);
                    result = await PerformOperation(operationInfos);
                }
                catch (Exception ex)
                {
                    result = new Result
                    {
                        Message = ex.Message,
                        StatusCode = Result.Status.Fail
                    };
                }
                finally
                {
                    FinalizeOperation();
                }

                result.ElapsedTime = DateTime.Now - startTime;
                return result;
            }
        }

        /// <summary>
        /// Performs an action when the abort button has been pressed.
        /// </summary>
        internal void AbortButtonAction()
        {
            try
            {
                if (Operation == null || !Operation.IsRunning)
                {
                    NavigateBackHome();
                }
                else
                {
                    IsCancelRequest = true;
                    Operation.Cancel();
                }
            }
            catch (ObjectDisposedException)
            {
                NavigateBackHome();
            }
        }

        /// <summary>
        /// Opens a picker to select a folder and returns it. 
        /// May be <code>null</code> on cancellation.
        /// </summary>
        internal async Task<StorageFolder> OutputPathPanelAction()
        {
            var picker = PickerFactory.CreateFolderPicker();

            var folder = await picker.PickSingleFolderAsync();
            if (folder != null) // system has now access to folder
            {
                OutputFolder = folder;
            }
            return folder;
        }

        /// <summary>
        /// Returns the name of a folder picked via folder picker.
        /// </summary>
        /// <returns>The name of the picked folder or 
        /// <code>string.Empty</code> if no folder was picked.</returns>
        internal async Task<string> PickOutputPath()
        {
            var folder = await OutputPathPanelAction();
            return folder?.Name ?? "";
        }

        /// <summary>
        /// Validates the specified size and displays a toast notification 
        /// if a threshold has been passed (<see cref="FileSizeWarningThreshold"/>).
        /// </summary>
        /// <param name="totalSize">The total size to be validated.</param>
        private void ValidateFileSizes(ulong totalSize)
        {
            if (totalSize >= FileSizeWarningThreshold)
            {
                ShowToastNotification("Please be patient", "This might take a while. . .");
            }
        }

        /// <summary>
        /// Checks the total size of all specified sizes and displays a toast notification 
        /// if a threshold has been passed (<see cref="FileSizeWarningThreshold"/>).
        /// </summary>
        /// <param name="item">The item whose size is to be checked.</param>
        protected async void CheckFileSizes(ExtractableItem item)
        {
            ulong totalSize = 0L;
            if (!item.Entries.IsNullOrEmpty())
            {
                totalSize = item.Entries.Aggregate(totalSize,
                    (current, entry) => current + entry.Size);
            }
            else
            {
                totalSize = await FileUtils.GetFileSizeAsync(item.Archive);
            }
            ValidateFileSizes(totalSize);
        }

        /// <summary>
        /// Checks the total size of all the files in the specified list and displays a 
        /// toast notification if a threshold has been passed (<see cref="FileSizeWarningThreshold"/>).
        /// </summary>
        /// <param name="files">The files whose sizes are to be checked.</param>
        protected async void CheckFileSizes(IReadOnlyList<StorageFile> files)
        {
            var totalSize = await FileUtils.GetFileSizesAsync(files);
            ValidateFileSizes(totalSize);
        }

        /// <summary>
        /// Performs various tasks before the start of the archiving operation.
        /// </summary>
        /// <param name="operationInfos">The amount of operations to be performed.</param>
        /// <exception cref="NullReferenceException">Thrown if output folder is <code>null</code>.</exception>
        protected void InitOperation(params T[] operationInfos)
        {
            if (OutputFolder == null)
            {
                throw new NullReferenceException("No valid output folder selected.");
            }
            // set output folder to each operation info
            foreach (var operationInfo in operationInfos)
            {
                operationInfo.OutputFolder = OutputFolder;
            }
            // keep display alive while operation is in progress
            DisplayRequest.RequestActive();
        }

        /// <summary>
        /// Performs various tasks after the archiving operation has finished.
        /// </summary>
        protected void FinalizeOperation()
        {
            IsCancelRequest = false;
            DisplayRequest.RequestRelease();
        }

        /// <summary>
        /// Returns a concrete instance of the archiving operation.
        /// </summary>
        /// <returns>A concrete instance of the archiving operation.</returns>
        protected abstract ArchivingOperation<T> GetArchivingOperation();

        /// <summary>
        /// Performs the archiving operation.
        /// </summary>
        /// <param name="operationInfos">The amount of operations to be performed.</param>
        /// <returns>True on success, false otherwise.</returns>
        protected abstract Task<Result> PerformOperation(T[] operationInfos);

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Operation?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
