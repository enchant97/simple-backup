using System;
using System.IO;

namespace SimpleBackup.Core
{
    public static class Paths
    {
        /// <summary>
        /// Combines a src path into the dest path
        /// will handle (and combine) drive letters for Windows.
        /// </summary>
        public static string CombineFullPath(string srcPath, string destPath)
        {
            bool isWindows = OperatingSystem.IsWindows();
            if (isWindows)
                return Path.Join(destPath, srcPath[0].ToString(), srcPath[3..]);
            return Path.Join(destPath, srcPath);
        }
        /// <summary>
        /// Generate a backname
        /// </summary>
        public static string GenerateBackupName()
        {
            string backupName = DateTime.UtcNow.ToString(Constants.BackupNameDateTimeFormat);
            backupName = Constants.BackupName + Constants.BackupNameSep + backupName;
            return backupName;
        }
        /// <summary>
        /// Generate a backname and join with root path
        /// </summary>
        /// <param name="rootPath">The root directory to join</param>
        public static string GenerateBackupName(string rootPath)
        {
            return Path.Join(rootPath, GenerateBackupName());
        }
        /// <summary>
        /// Generate a backname with backup type
        /// </summary>
        /// <param name="backupType">The backup type</param>
        public static string GenerateBackupName(Constants.BackupType backupType)
        {
            string backupName = GenerateBackupName();
            switch (backupType)
            {
                case Constants.BackupType.ZIP:
                case Constants.BackupType.ZIP_NO_COMPRESS:
                    backupName += ".zip";
                    break;

            }
            return backupName;
        }
        /// <summary>
        /// Generate a backname and join with root path
        /// </summary>
        /// <param name="rootPath">The root directory to join</param>
        /// <param name="backupType">The backup type</param>
        public static string GenerateBackupName(string rootPath, Constants.BackupType backupType)
        {
            return Path.Join(rootPath, GenerateBackupName(backupType));
        }
    }
}
