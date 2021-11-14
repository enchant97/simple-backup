using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;

namespace SimpleBackup.Core.Backup
{
    public class BackupHandler
    {
        #region Fields
        private readonly ConcurrentQueue<string> pathsLeft;
        private bool isBackupInProgress = false;
        private bool isPaused = false;
        private readonly string[] includedPaths;
        private readonly string[] excludedPaths;
        private readonly string[] excludedRegexFilenames;
        private readonly string destinationPath;
        private readonly Constants.BackupType backupType;
        private readonly bool pauseOnError;
        /// <summary>
        /// Check whether a backup is in progress
        /// </summary>
        public bool IsBackupInProgress { get => isBackupInProgress; }
        /// <summary>
        /// Check whether the backup has been paused
        /// </summary>
        public bool IsPaused { get => isPaused; }
        /// <summary>
        /// Check whether the paths to backup queue is empty
        /// </summary>
        public bool IsPathQueueEmpty { get => pathsLeft.IsEmpty; }
        #endregion
        #region Private Methods
        private void InitQueue()
        {
            pathsLeft.Clear();
            foreach (var searchPath in includedPaths)
            {
                try
                {
                    foreach (var foundFilePath in Discovery.SearchFilesEnumerated(searchPath, excludedPaths, excludedRegexFilenames))
                    {
                        if (IsPaused) { break; }
                        pathsLeft.Enqueue(foundFilePath);
                        DiscoveryEvent?.Invoke(this, new BackupHandlerEventArgs(foundFilePath));
                    }
                }
                catch (Exception ex)
                {
                    HandleDiscoveringExceptions(ex, searchPath);
                }
            }
        }
        private void HandleDiscoveringExceptions(Exception exception, string searchPath)
        {
            if (pauseOnError) { Pause(); }
            if (exception is FileNotFoundException)
            {
                ExceptionDiscoveringEvent?.Invoke(this, new BackupHandlerErrorEventArgs(searchPath, Constants.ErrorTypes.NOT_FOUND));
            }
            else if (exception is IOException)
            {
                ExceptionDiscoveringEvent?.Invoke(this, new BackupHandlerErrorEventArgs(searchPath, Constants.ErrorTypes.NOT_COPYABLE_TYPE));
            }
            else if (exception is UnauthorizedAccessException)
            {
                ExceptionDiscoveringEvent?.Invoke(this, new BackupHandlerErrorEventArgs(searchPath, Constants.ErrorTypes.NO_PERMISSION));
            }
            else
            {
                ExceptionDiscoveringEvent?.Invoke(this, new BackupHandlerErrorEventArgs(searchPath, Constants.ErrorTypes.UNHANDLED));
                throw exception;
            }
        }
        private void HandleCopyExceptions(Exception exception, string fileName)
        {
            if (pauseOnError) { Pause(); }
            if (exception is FileNotFoundException)
            {
                ExceptionCopyEvent?.Invoke(this, new BackupHandlerErrorEventArgs(fileName, Constants.ErrorTypes.NOT_FOUND));
            }
            else if (exception is IOException)
            {
                ExceptionCopyEvent?.Invoke(this, new BackupHandlerErrorEventArgs(fileName, Constants.ErrorTypes.NOT_COPYABLE_TYPE));
            }
            else if (exception is UnauthorizedAccessException)
            {
                ExceptionCopyEvent?.Invoke(this, new BackupHandlerErrorEventArgs(fileName, Constants.ErrorTypes.NO_PERMISSION));
            }
            else
            {
                ExceptionCopyEvent?.Invoke(this, new BackupHandlerErrorEventArgs(fileName, Constants.ErrorTypes.UNHANDLED));
                throw exception;
            }
        }
        private void CopyAsDirectory()
        {
            string fileName = null;
            try
            {
                while (!IsPathQueueEmpty && !IsPaused)
                {
                    bool isValid = pathsLeft.TryDequeue(out fileName);
                    if (!isValid) { return; }

                    string fileDstPath = Paths.CombineFullPath(fileName, destinationPath);
                    Directory.CreateDirectory(Path.GetDirectoryName(fileDstPath));
                    File.Copy(fileName, fileDstPath);
                    CopyEvent?.Invoke(this, new BackupHandlerEventArgs(fileName));
                }
            }
            catch (Exception ex)
            {
                HandleCopyExceptions(ex, fileName);
            }
        }
        private void CopyAsZip()
        {
            // TODO rewrite using SharpZipLib
            string fileName = null;
            CompressionLevel compressionLevel = backupType switch
            {
                Constants.BackupType.ZIP_NO_COMPRESS => CompressionLevel.NoCompression,
                _ => CompressionLevel.Optimal,
            };
            try
            {
                using FileStream zipStream = File.Open(destinationPath, FileMode.OpenOrCreate);
                using ZipArchive archive = new(zipStream, ZipArchiveMode.Update);
                while (!IsPathQueueEmpty && !IsPaused)
                {
                    bool isValid = pathsLeft.TryDequeue(out fileName);
                    if (!isValid) { return; }
                    string dstPath = Paths.CombineFullPath(fileName, "");
                    archive.CreateEntryFromFile(fileName, dstPath, compressionLevel);
                    CopyEvent?.Invoke(this, new BackupHandlerEventArgs(fileName));
                }
            }
            catch (Exception ex)
            {
                HandleCopyExceptions(ex, fileName);
            }

        }
        private void StartCopy()
        {
            try
            {
                switch (backupType)
                {
                    case Constants.BackupType.FOLDER:
                        CopyAsDirectory();
                        break;
                    case Constants.BackupType.ZIP:
                    case Constants.BackupType.ZIP_NO_COMPRESS:
                        CopyAsZip();
                        break;
                    default:
                        throw new Exception(string.Format("backup type '{0}' not supported", backupType.ToString()));
                }
            }
            catch (Exception ex)
            {
                HandleCopyExceptions(ex, null);
            }
        }
        #endregion
        #region Public Methods
        /// <summary>Create a backup handler object, defaulting to FOLDER backup</summary>
        public BackupHandler(
            string destinationPath,
            string[] includedPaths,
            string[] excludedPaths,
            string[] excludedRegexFilenames,
            bool pauseOnError)
        {
            this.destinationPath = destinationPath;
            this.includedPaths = includedPaths;
            this.excludedPaths = excludedPaths;
            this.excludedRegexFilenames = excludedRegexFilenames;
            backupType = Constants.BackupType.FOLDER;
            this.pauseOnError = pauseOnError;
            pathsLeft = new();
        }
        /// <summary>Create a backup handler object</summary>
        public BackupHandler(
            string destinationPath,
            string[] includedPaths,
            string[] excludedPaths,
            string[] excludedRegexFilenames,
            Constants.BackupType backupType,
            bool pauseOnError) : this(destinationPath, includedPaths, excludedPaths, excludedRegexFilenames, pauseOnError)
        {
            this.backupType = backupType;
        }
        /// <summary>Start the backup</summary>
        public void Start()
        {
            // make sure a backup is not running when starting
            if (IsBackupInProgress && !IsPaused)
                throw new IsBackupInProgressException("Backup can't be started when one is in progress");

            StartedEvent?.Invoke(this, EventArgs.Empty);
            isBackupInProgress = true;

            // check for whether the backup was paused or is new
            if (IsPaused)
                isPaused = false;
            else
                InitQueue();

            // TODO use thread pool for copy (but make start method still block)
            // copy paths until finished or paused
            StartCopy();

            if (IsPathQueueEmpty)
            {
                isBackupInProgress = false;
                isPaused = false;
                FinishedEvent?.Invoke(this, EventArgs.Empty);
            }
        }
        /// <summary>Stop a ongoing backup remembering progress</summary>
        public void Pause()
        {
            if (!IsBackupInProgress)
                throw new NoIsBackupInProgressException("Backup can't be paused when none is running");
            isPaused = true;
            PausedEvent?.Invoke(this, EventArgs.Empty);
        }
        /// <summary>Stop a ongoing backup (will reset progress)</summary>
        public void Stop()
        {
            if (!IsBackupInProgress)
                throw new NoIsBackupInProgressException("Backup can't be stopped when none is running");
            pathsLeft.Clear();
        }
        #endregion
        #region Events
        /// <summary>Invoked when a path is discovered</summary>
        public event EventHandler<BackupHandlerEventArgs> DiscoveryEvent;
        /// <summary>Invoked when a path has finished copying</summary>
        public event EventHandler<BackupHandlerEventArgs> CopyEvent;
        /// <summary>Invoked when discovery had a error</summary>
        public event EventHandler<BackupHandlerErrorEventArgs> ExceptionDiscoveringEvent;
        /// <summary>Invoked when copy had a error</summary>
        public event EventHandler<BackupHandlerErrorEventArgs> ExceptionCopyEvent;
        /// <summary>Invoked when the backup is started</summary>
        public event EventHandler StartedEvent;
        /// <summary>Invoked when the backup has been paused</summary>
        public event EventHandler PausedEvent;
        /// <summary>Invoked when backup is finished</summary>
        public event EventHandler FinishedEvent;
        #endregion
    }
}
