using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;

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
        private void HandleBackupExceptions(EventHandler<BackupHandlerErrorEventArgs> eventHandler, Exception exception, string path)
        {
            if (exception is FileNotFoundException)
            {
                eventHandler?.Invoke(this, new BackupHandlerErrorEventArgs(path, Constants.ErrorTypes.NOT_FOUND)); ;
            }
            else if (exception is IOException)
            {
                eventHandler?.Invoke(this, new BackupHandlerErrorEventArgs(path, Constants.ErrorTypes.NOT_COPYABLE_TYPE));
            }
            else if (exception is UnauthorizedAccessException)
            {
                eventHandler?.Invoke(this, new BackupHandlerErrorEventArgs(path, Constants.ErrorTypes.NO_PERMISSION));
            }
            else
            {
                eventHandler?.Invoke(this, new BackupHandlerErrorEventArgs(path, Constants.ErrorTypes.UNHANDLED));
                throw exception;
            }
        }
        private void HandleDiscoveringExceptions(Exception exception, string searchPath)
        {
            if (pauseOnError) { Pause(); }
            HandleBackupExceptions(ExceptionDiscoveringEvent, exception, searchPath);
        }
        private void HandleCopyExceptions(Exception exception, string fileName)
        {
            if (pauseOnError) { Pause(); }
            HandleBackupExceptions(ExceptionCopyEvent, exception, fileName);
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

                    string fileDstPath = Paths.Generation.CombineFullPath(fileName, destinationPath);
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
            string fileName = null;
            CompressionMethod compressionMethod = backupType switch
            {
                Constants.BackupType.ZIP_NO_COMPRESS => CompressionMethod.Stored,
                _ => CompressionMethod.Deflated,
            };
            try
            {
                using FileStream fs = File.Open(destinationPath, FileMode.OpenOrCreate);
                using ZipOutputStream outStream = new(fs);
                while (!IsPathQueueEmpty && !IsPaused)
                {
                    bool isValid = pathsLeft.TryDequeue(out fileName);
                    if (!isValid) { return; }
                    string dstPath = Paths.Generation.CombineFullPath(fileName, "");
                    outStream.PutNextEntry(new ZipEntry(dstPath) { CompressionMethod = compressionMethod });
                    outStream.Write(File.ReadAllBytes(fileName));
                    CopyEvent?.Invoke(this, new BackupHandlerEventArgs(fileName));
                }
            }
            catch (Exception ex)
            {
                HandleCopyExceptions(ex, fileName);
            }
        }
        private void CopyAsTar()
        {
            string fileName = null;
            try
            {
                using FileStream fs = File.Open(destinationPath, FileMode.OpenOrCreate);
                using Stream outStream = backupType switch
                {
                    Constants.BackupType.TAR_GZ => new GZipOutputStream(fs),
                    _ => new TarOutputStream(fs, Encoding.UTF8),
                };
                using TarArchive tarArchive = TarArchive.CreateOutputTarArchive(outStream);
                while (!IsPathQueueEmpty && !IsPaused)
                {
                    bool isValid = pathsLeft.TryDequeue(out fileName);
                    if (!isValid) { return; }
                    string dstPath = Paths.Generation.CombineFullPath(fileName, "");
                    TarEntry tarEntry = TarEntry.CreateEntryFromFile(fileName);
                    tarEntry.Name = dstPath;
                    tarArchive.WriteEntry(tarEntry, false);
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
                    case Constants.BackupType.TAR:
                    case Constants.BackupType.TAR_GZ:
                        CopyAsTar();
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
