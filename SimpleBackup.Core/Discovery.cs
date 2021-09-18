using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SimpleBackup.Core
{
    public static class Discovery
    {
        /// <summary>
        /// Check whether a child path is part of a parent path
        /// </summary>
        public static bool IsPathPartOf(string possibleChild, string possibleParent)
        {
            if (possibleChild.StartsWith(possibleParent))
                return true;
            return false;
        }
        /// <summary>
        /// Check whether a child path is part of any given parent paths
        /// </summary>
        public static bool IsPathPartOf(string possibleChild, string[] possibleParents)
        {
            foreach (var possibleParent in possibleParents)
            {
                if (IsPathPartOf(possibleChild, possibleParent))
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Search for all any file from a starting directory
        /// </summary>
        public static IEnumerable<string> SearchFilesEnumerated(string startingDirectory)
        {
            return Directory.EnumerateFiles(startingDirectory, "*", SearchOption.AllDirectories);
        }
        /// <summary>
        /// Search for all any file from a starting directory,
        /// skipping any files that are part of the given excluded paths
        /// </summary>
        public static IEnumerable<string> SearchFilesEnumerated(string startingDirectory, string[] excludedPaths)
        {
            var files = SearchFilesEnumerated(startingDirectory);
            foreach (var file in files)
            {
                if (!IsPathPartOf(file, excludedPaths))
                    yield return file;
            }
        }
        /// <summary>
        /// Find previous folder backups in a directory
        /// </summary>
        public static IEnumerable<string> FindPreviousBackupFolders(string backupDirectory)
        {
            var folders = Directory.EnumerateDirectories(backupDirectory, "*", SearchOption.TopDirectoryOnly);
            Regex regex = new(Constants.BackupNameFolderRegex);
            foreach (var folderPath in folders)
            {
                string folderName = Path.GetFileName(folderPath);
                if (regex.IsMatch(folderName))
                    yield return folderName;
            }
        }
        /// <summary>
        /// Find previous file backups in a directory.
        /// </summary>
        public static IEnumerable<string> FindPreviousBackupFiles(string backupDirectory)
        {
            var files = Directory.EnumerateFiles(backupDirectory, "*", SearchOption.TopDirectoryOnly);
            Regex regex = new(Constants.BackupNameFileRegex);
            foreach (var filePath in files)
            {
                string fileName = Path.GetFileName(filePath);
                if (regex.IsMatch(fileName))
                    yield return fileName;
            }
        }
    }
}
