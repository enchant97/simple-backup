using System.Text.RegularExpressions;

namespace SimpleBackup.Core.Paths
{
    public static class Checkers
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
        /// Check whether a path matches regular expression
        /// </summary>
        public static bool IsPathMatchRegex(string filePath, string[] regexPatterns)
        {
            foreach (var regexPattern in regexPatterns)
            {
                Regex regex = new(regexPattern);
                if (regex.IsMatch(filePath))
                    return true;
            }
            return false;
        }
    }
}
