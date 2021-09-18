using System;
using System.IO;
using System.Linq;

namespace SimpleBackup.Core
{
    public static class Cleaning
    {
        /// <summary>
        /// Remove any previous backups while keeping a
        /// certain number versions around
        /// </summary>
        /// <param name="versionsToKeep">
        /// Number of backups to keep,
        /// must be a positive number
        /// </param>
        /// <param name="backupDirectory">
        /// Where the backups can be found
        /// </param>
        /// <returns>How many backups were deleted</returns>
        /// <exception cref="System.Exception">
        /// When versionsToKeep is below 1
        /// </exception>
        public static int RemovePreviousBackups(int versionsToKeep, string backupDirectory)
        {
            // argument validation check
            if (versionsToKeep < 0)
                throw new System.Exception("Argument 'versionsToKeep' must be >= 0");

            // get both the file & folder backups
            string[] allBackups = Discovery.FindPreviousBackups(backupDirectory).ToArray();
            int backupsLength = allBackups.Length;

            // make sure there is enough to remove
            if (backupsLength <= versionsToKeep)
                return 0;

            // make sure newest backups are first (Decending)
            Array.Sort(allBackups);

            for (int i = versionsToKeep; i < backupsLength; i++)
            {
                string currPath = allBackups[i];
                string fullPath = Path.Join(backupDirectory, currPath);

                FileAttributes attributes = File.GetAttributes(fullPath);
                // faster than attributes.HasFlag()
                if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
                    Directory.Delete(fullPath, true);
                else
                    File.Delete(fullPath);
            }
            return backupsLength - versionsToKeep;
        }
    }
}
