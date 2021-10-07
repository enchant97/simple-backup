using System;

namespace SimpleBackup.Core.Backup
{
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
        public Constants.ErrorTypes errorType { get; set; }
        public BackupHandlerErrorEventArgs(string fullPath, Constants.ErrorTypes errorType) : base(fullPath)
        {
            this.errorType = errorType;
        }
    }
}