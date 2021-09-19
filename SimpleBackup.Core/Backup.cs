using System;
using System.Collections.Concurrent;
using System.IO;

namespace SimpleBackup.Core.Backup
{
    public enum ErrorTypes
    {
        UNHANDLED,
        NOT_FOUND,
        NOT_COPYABLE_TYPE,
        NO_PERMISSION,
    }
    #region Exceptions
    public class NoIsBackupInProgressException : Exception
    {
        public NoIsBackupInProgressException() : base() { }
        public NoIsBackupInProgressException(string message) : base(message) { }
    }
    public class IsBackupInProgressException : Exception
    {
        public IsBackupInProgressException() : base() { }
        public IsBackupInProgressException(string message) : base(message) { }
    }
    #endregion
    #region EventArgs
    public class BackupHandlerEventArgs : EventArgs
    {
        public string fullPath { get; set; }
        public BackupHandlerEventArgs(string fullPath)
        {
            this.fullPath = fullPath;
        }
    }
    public class BackupHandlerErrorEventArgs : BackupHandlerEventArgs
    {
        public ErrorTypes errorType { get; set; }
        public BackupHandlerErrorEventArgs(string fullPath, ErrorTypes errorType) : base(fullPath)
        {
            this.errorType = errorType;
        }
    }
    #endregion
    public class BackupHandler
    {
        #region Fields
        private ConcurrentQueue<string> pathsLeft;
        private bool isBackupInProgress = false;
        private bool isPaused = false;
        private readonly string[] includedPaths;
        private readonly string[] excludedPaths;
        private readonly string destinationPath;
        private readonly bool pauseOnError;
        public bool IsBackupInProgress { get => isBackupInProgress; }
        public bool IsPaused { get => isPaused; }
        public bool IsPathQueueEmpty { get => pathsLeft.IsEmpty; }
        #endregion
        #region Private Methods
        private void InitQueue()
        {
            // TODO add error handling
            pathsLeft.Clear();
            foreach (var searchPath in includedPaths)
            {
                foreach (var foundFilePath in Discovery.SearchFilesEnumerated(searchPath, excludedPaths))
                {
                    pathsLeft.Enqueue(foundFilePath);
                    DiscoveryEvent?.Invoke(this, new BackupHandlerEventArgs(foundFilePath));
                }
            }
        }
        private void Copy(string fileName)
        {
            // TODO add error handling
            string fileDstPath = Paths.CombineFullPath(fileName, destinationPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fileDstPath));
            File.Copy(fileName, fileDstPath);
            CopyEvent?.Invoke(this, new BackupHandlerEventArgs(fileName));
        }
        #endregion
        ///<summary>Create a backup handler object</summary>
        public BackupHandler(
            string destinationPath,
            string[] includedPaths,
            string[] excludedPaths,
            bool pauseOnError)
        {
            this.destinationPath = destinationPath;
            this.includedPaths = includedPaths;
            this.excludedPaths = excludedPaths;
            this.pauseOnError = pauseOnError;
            pathsLeft = new();
        }
        ///<summary>Start the backup</summary>
        public void Start()
        {
            // make sure a backup is not running when starting
            if (isBackupInProgress && !isPaused)
                throw new IsBackupInProgressException();

            StartedEvent?.Invoke(this, EventArgs.Empty);
            isBackupInProgress = true;

            // check for whether the backup was paused or is new
            if (IsPaused)
                isPaused = false;
            else
                InitQueue();

            // copy paths until finished or paused
            while (!IsPathQueueEmpty && !IsPaused)
            {
                bool isValid = pathsLeft.TryDequeue(out string currPath);
                if (isValid)
                    Copy(currPath);
            }

            if (IsPathQueueEmpty)
            {
                isBackupInProgress = false;
                isPaused = false;
                FinishedEvent?.Invoke(this, EventArgs.Empty);
            }
        }
        ///<summary>Stop a ongoing backup remembering progress</summary>
        public void Pause()
        {
            if (!isBackupInProgress)
                throw new NoIsBackupInProgressException();
            isPaused = true;
            PausedEvent?.Invoke(this, EventArgs.Empty);
        }
        ///<summary>Stop a ongoing backup (will reset progress)</summary>
        public void Stop()
        {
            if (!isBackupInProgress)
                throw new NoIsBackupInProgressException();
            pathsLeft.Clear();
        }
        #region Events
        ///<summary>Invoked when a path is discovered</summary>
        public event EventHandler<BackupHandlerEventArgs> DiscoveryEvent;
        ///<summary>Invoked when a path has finished copying</summary>
        public event EventHandler<BackupHandlerEventArgs> CopyEvent;
        ///<summary>Invoked when copy or discovery had a error</summary>
        public event EventHandler<BackupHandlerErrorEventArgs> ExceptionEvent;
        ///<summary>Invoked when the backup is started</summary>
        public event EventHandler StartedEvent;
        ///<summary>Invoked when the backup has been paused</summary>
        public event EventHandler PausedEvent;
        ///<summary>Invoked when backup is finished</summary>
        public event EventHandler FinishedEvent;
        #endregion
    }
}
