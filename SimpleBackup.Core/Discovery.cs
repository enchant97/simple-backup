using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SimpleBackup.Core
{
    public static class Discovery
    {
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
            IEnumerable<string> files = SearchFilesEnumerated(startingDirectory);
            foreach (string file in files)
            {
                if (!Paths.Checkers.IsPathPartOf(file, excludedPaths))
                    yield return file;
            }
        }
        /// <summary>
        /// Search for all any file from a starting directory,
        /// skipping any files that are part of the given excluded paths
        /// and skipping filenames that match given regex
        /// </summary>
        public static IEnumerable<string> SearchFilesEnumerated(
            string startingDirectory,
            string[] excludedPaths,
            string[] excludedRegexFilenames)
        {
            IEnumerable<string> files = SearchFilesEnumerated(startingDirectory);
            foreach (string file in files)
            {
                if (!Paths.Checkers.IsPathPartOf(file, excludedPaths))
                    if (!Paths.Checkers.IsPathMatchRegex(Path.GetFileName(file), excludedRegexFilenames))
                        yield return file;
            }
        }
        /// <summary>
        /// Find previous folder backups in a directory
        /// </summary>
        public static IEnumerable<string> FindPreviousBackupFolders(string backupDirectory)
        {
            IEnumerable<string> folders = Directory.EnumerateDirectories(backupDirectory, "*", SearchOption.TopDirectoryOnly);
            Regex regex = new(Constants.BackupNameFolderRegex);
            foreach (string folderPath in folders)
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
            IEnumerable<string> files = Directory.EnumerateFiles(backupDirectory, "*", SearchOption.TopDirectoryOnly);
            Regex regex = new(Constants.BackupNameFileRegex);
            foreach (string filePath in files)
            {
                string fileName = Path.GetFileName(filePath);
                if (regex.IsMatch(fileName))
                    yield return fileName;
            }
        }
        /// <summary>
        /// Find previous file and folder backups in a directory.
        /// </summary>
        public static IEnumerable<string> FindPreviousBackups(string backupDirectory)
        {
            return FindPreviousBackupFiles(backupDirectory).Concat(
                FindPreviousBackupFolders(backupDirectory));
        }
    }
}
