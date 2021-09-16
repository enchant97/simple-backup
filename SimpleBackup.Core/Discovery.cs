using System.IO;
using System.Collections.Generic;

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
                if (possibleChild.StartsWith(possibleParent))
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
            var files = Directory.EnumerateFiles(startingDirectory, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if(!IsPathPartOf(file, excludedPaths))
                    yield return file;
            }
        }
    }
}
