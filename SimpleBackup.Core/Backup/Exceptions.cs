using System;

namespace SimpleBackup.Core.Backup
{
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
}