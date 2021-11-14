using System;

namespace SimpleBackup.Core.Backup
{
    public class BackupHandlerEventArgs : EventArgs
    {
        public string FullPath { get; init; }
        public BackupHandlerEventArgs(string fullPath)
        {
            FullPath = fullPath;
        }
    }
    public class BackupHandlerErrorEventArgs : BackupHandlerEventArgs
    {
        public Constants.ErrorTypes ErrorType { get; init; }
        public BackupHandlerErrorEventArgs(string fullPath, Constants.ErrorTypes errorType) : base(fullPath)
        {
            ErrorType = errorType;
        }
    }
}