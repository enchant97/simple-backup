using System;
using System.IO;

namespace SimpleBackup.Core
{
    public static class Constants
    {
        public const string AppName = "Simple Backup";
        public const string ConfigFilename = "config.xml";
        public static readonly string UserHomePath = Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config",
            "simple-backup"
        );
        public static readonly string ConfigFullPath = Path.Join(
            UserHomePath,
            ConfigFilename
        );
    }
}
